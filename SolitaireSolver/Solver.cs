using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolitaireSolver.Objects;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SolitaireSolver {
	class Solver {
		const int NumThreads = 16;
		const int NumPriorities = 6;
		const int MaxQueued = (int)1e+7;
		// create queues
		// priority 0 is moves that flip face-down cards
		// priority 1 is moves that put a card on the foundations
		// priority 2 is moves that move cards around on the tableau
		// priority 3 is moves that move a card from the waste to the tableau
		// priority 4 is iterating through the deck
		// priority 5 is moves that move a card from one of the foundations to the tableau (if allowed)
		static ConcurrentPriorityQueue<int, State> queue;
		static ConcurrentDictionary<int, Object> states;
		static Thread[] threads;
		static Semaphore sem;
		static bool solved;

		static int statesEnqueued;
		static int threadsWaiting;
		static int mostFoundationed;
		static State bestState;
		static Stopwatch stopWatch;

		static void Main(string[] args) {
			bool foundOne = false;
			int seed = 0;
			while(!foundOne) {
				Console.WriteLine("Trying seed {0}", seed);
				foundOne = SolveBoard(seed++);
			}
			
		}

		/// <summary>
		/// Attempts to solve a board with the given seed
		/// </summary>
		/// <param name="seed">Seed to generate initial state</param>
		/// <returns>True if solved, false otherwise</returns>
		static bool SolveBoard(int seed) {
			sem = new Semaphore(1, int.MaxValue);
			queue = new ConcurrentPriorityQueue<int, State>();
			states = new ConcurrentDictionary<int, Object>();
			threads = new Thread[NumThreads];
			solved = false;
			statesEnqueued = 1;
			threadsWaiting = 0;
			mostFoundationed = 0;
			bestState = new State();
			stopWatch = new Stopwatch();
			// generate a random board
			State initialState = new State();
			initialState.Deal(seed);
			//initialState.MakeSolveable();
			queue.Enqueue(0, initialState);
			states.TryAdd(initialState.GetHashCode(), null);

			stopWatch.Start();

			for(int i = 0; i < NumThreads; i++) {
				threads[i] = new Thread(Solve);
				threads[i].Start();
			}

			// wait for all threads to finish
			for(int i = 0; i < NumThreads; i++)
				threads[i].Join();
			if(!solved)
				Console.WriteLine("Unable to find a solution after trying {0} states. Best state was {1}.", statesEnqueued, mostFoundationed);
			return solved;
		}

		static void Solve() {
			while(!solved) {
				State state = new State();
				Interlocked.Increment(ref threadsWaiting);
				if(statesEnqueued > MaxQueued)
					return;
				while(!sem.WaitOne(500)) {
					if(threadsWaiting >= NumThreads)
						return;
				}

				// pull a state from the first queue that has one
				System.Collections.Generic.KeyValuePair<int, State> statePair;
				while(!queue.TryDequeue(out statePair));
				state = statePair.Value;
				
				// then find all possible moves
				// first try to flip a card on the tableau
				for(int i = 0; i < State.NumTableaus; i++) {
					if(state.Tableaus[i].Count != 0) {
						Card toFlip = state.Tableaus[i].Last();
						if(!toFlip.IsFaceUp) {
							Move flipMove = new Move(i);
							State newState = new State(state);
							newState.ExecuteMove(flipMove, false);
							RegisterState(newState, 0);
						}
					}
				}

				// then try to move a card from the tableau to its foundation
				for(int i = 0; i < State.NumTableaus; i++) {
					List<Card> tableau = state.Tableaus[i];
					if(tableau.Count != 0) {
						Move tableauFoundationMove = new Move(Move.Zone.Tableaus, i, tableau.Count - 1, Move.Zone.Foundations, tableau[tableau.Count - 1].CardSuit);
						if(state.IsMoveLegal(tableauFoundationMove)) {
							State newState = new State(state);
							newState.ExecuteMove(tableauFoundationMove, false);
							// register the new state
							RegisterState(newState, 1);
						}
					}
				}

				// then try to move from the waste to the foundation (if the waste isn't empty)
				if(state.Waste.Count != 0) {
					Move wasteFoundationMove = new Move(Move.Zone.Waste, 0, 0, Move.Zone.Foundations, state.Waste.Peek().CardSuit);
					if(state.IsMoveLegal(wasteFoundationMove)) {
						State newState = new State(state);
						newState.ExecuteMove(wasteFoundationMove, false);
						RegisterState(newState, 1);
					}
				}

				// then move cards around on the tableau
				for(int i = 0; i < State.NumTableaus; i++) {
					List<Card> tableau = state.Tableaus[i];
					for(int j = 0; j < tableau.Count; j++) {
						Card first = tableau[j];
						foreach(int k in LookForSuitableTableaus(first, state)) {
							Move tableauMove = new Move(Move.Zone.Tableaus, i, j, Move.Zone.Tableaus, k);
							State newState = new State(state);
							newState.ExecuteMove(tableauMove, false);
							RegisterState(newState, 2);
						}
					}
				}

				// then move cards from the waste to the tableau (if any)
				if(state.Waste.Count != 0) {
					Card wasted = state.Waste.Peek();
					foreach(int k in LookForSuitableTableaus(wasted, state)) {
						Move wasteMove = new Move(Move.Zone.Waste, 0, 0, Move.Zone.Tableaus, k);
						State newState = new State(state);
						newState.ExecuteMove(wasteMove, false);
						RegisterState(newState, 3);
					}
				}

				// try iterating through the deck
				Move iterateMove = new Move(Move.Type.IterateWaste);
				State iterateState = new State(state);
				iterateState.ExecuteMove(iterateMove, false);
				RegisterState(iterateState, 4);

				// finally, if allowed, move cards from the foundation to the tableau
				if(State.CanTakeFromFoundations) {
					foreach(Suit suit in State.Suits) {
						if(state.Foundations[suit].Count == 0)
							continue;
						Card top = state.Foundations[suit].Peek();
						foreach(int k in LookForSuitableTableaus(top, state)) {
							Move foundationMove = new Move(Move.Zone.Foundations, suit, 0, Move.Zone.Tableaus, k);
							State newState = new State(state);
							newState.ExecuteMove(foundationMove, false);
							RegisterState(newState, 5);
						}
					}
				}
			}
		}

		/// <summary>
		/// Looks through all tableaus, attempting to find a suitable target for the given card
		/// </summary>
		/// <param name="bottom">Bottom card in the stack to look with</param>
		/// <param name="state">State to look at</param>
		/// <returns>List of indices of tableaus that can take this card</returns>
		static IEnumerable<int> LookForSuitableTableaus(Card bottom, State state) {
			if(!bottom.IsFaceUp)
				yield break;
			for(int k = 0; k < State.NumTableaus; k++) {
				if(state.Tableaus[k].Count != 0) {
					Card top = state.Tableaus[k].Last();
					if(top.IsFaceUp && (top.CardValue - 1 == bottom.CardValue) && (top.CardSuit.Color != bottom.CardSuit.Color))
						yield return k;
				}
			}
		}

		/// <summary>
		/// Adds the state to the queue and dictionary if it hasn't already been seen. Also checks for a win.
		/// </summary>
		/// <param name="toRegister"></param>
		/// <param name="priority"></param>
		static void RegisterState(State toRegister, int priority) {
			LookForWin(toRegister);
			if(states.TryAdd(toRegister.GetHashCode(), null)) {
				// this is a new state
				priority = 52 * NumPriorities - (toRegister.FoundationOccupancy() * NumPriorities + (NumPriorities - priority));
				queue.Enqueue(priority, toRegister);
				sem.Release();
				if(toRegister.FoundationOccupancy() > mostFoundationed) {
					mostFoundationed = toRegister.FoundationOccupancy();
					bestState = toRegister;
				}
				if(Interlocked.Increment(ref statesEnqueued) % 100000 == 0)
					Console.WriteLine("{0} enq, {1} curr, {2} best, {3} s", statesEnqueued, queue.Count, mostFoundationed, stopWatch.Elapsed.TotalSeconds.ToString("F2"));
			} else {
				// this state has been encountered before
			}
		}

		/// <summary>
		/// Looks for a win in this state, printing the solution and terminating threads if so
		/// </summary>
		/// <param name="state">State to check</param>
		static void LookForWin(State state) {
			// see if this state is solved
			if(state.CheckForWin()) {
				solved = true;
				Console.WriteLine("Solved in {0} s!", stopWatch.Elapsed.TotalSeconds);
				Console.WriteLine("Steps:");
				foreach(Move move in state.Moves)
					Console.WriteLine(move.ToString());
			}
		}
	}
}
