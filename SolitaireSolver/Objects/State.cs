using SolitaireSolver.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver.Objects {
	class State {
		public static Suit[] Suits = {
			new Suit("Clubs", Suit.Colors.Black, '♣'),
			new Suit("Spades", Suit.Colors.Black, '♠'),
			new Suit("Hearts", Suit.Colors.Red, '♥'),
			new Suit("Diamonds", Suit.Colors.Red, '♦')
		};

		public static byte NumTableaus = 7;
		// the number of cards that should be drawn from the deck to the waste
		public static byte DrawNumber = 1;
		// whether or not you're allowed to take cards from the foundations after they've been put there
		public static bool CanTakeFromFoundations = false;

		// the fountdations are Lists rather than Stacks because you're allowed to see and perform moves on more than just the top card.
		public Dictionary<Suit, Stack<Card>> Foundations;
		public List<Card>[] Tableaus;
		public Stack<Card> Waste;
		public Stack<Card> Deck;

		// contains all the moves taken from the initial state to get to this state.
		// Used for printing the solution after finding it.
		public Queue<Move> Moves;

		/// <summary>
		/// Creates a new empty State.
		/// </summary>
		public State() {
			Foundations = new Dictionary<Suit, Stack<Card>>();
			foreach(Suit suit in Suits)
				Foundations[suit] = new Stack<Card>();
			Tableaus = new List<Card>[NumTableaus];
			for(int i = 0; i < NumTableaus; i++)
				Tableaus[i] = new List<Card>();
			Waste = new Stack<Card>();
			Deck = new Stack<Card>();

			Moves = new Queue<Move>();
		}

		/// <summary>
		/// Copies State into a new State.
		/// </summary>
		/// <param name="other">State to copy</param>
		public State(State other) {
			Stack<Card> temp;
			Foundations = new Dictionary<Suit, Stack<Card>>();
			foreach(Suit suit in Suits) {
				// copying a stack object to another stack using the copy constructor in c# actually flips it,
				// so we need to flip it back.
				temp = new Stack<Card>(other.Foundations[suit]);
				Foundations[suit] = new Stack<Card>();
				while(temp.Count != 0) {
					Card card = new Card(temp.Pop());
					Foundations[suit].Push(card);
				}
			}

			Tableaus = new List<Card>[NumTableaus];
			for(int i = 0; i < NumTableaus; i++) {
				Tableaus[i] = other.Tableaus[i].ConvertAll(card => new Card(card));
			}

			temp = new Stack<Card>(other.Waste);
			Waste = new Stack<Card>();
			while(temp.Count != 0) {
				Card card = new Card(temp.Pop());
				Waste.Push(card);
			}

			temp = new Stack<Card>(other.Deck);
			Deck = new Stack<Card>();
			while(temp.Count != 0) {
				Card card = new Card(temp.Pop());
				Deck.Push(card);
			}

			Queue<Move> q = new Queue<Move>(other.Moves);
			Moves = new Queue<Move>();
			while(q.Count != 0) {
				Move move = new Move(q.Dequeue());
				Moves.Enqueue(move);
			}
		}

		/// <summary>
		/// Generates a known-solveable game.
		/// </summary>
		public void MakeSolveable() {
			Dictionary<Suit, Stack<Card>> cards = new Dictionary<Suit, Stack<Card>>();
			foreach(Suit suit in Suits) {
				cards[suit] = new Stack<Card>();
				for(int i = 13; i > 0; i--)
					cards[suit].Push(new Card(suit, i));
			}

			Stack<Card> temp = new Stack<Card>();

			List<Card> t = Tableaus[0];
			t.Add(cards[Suits[0]].Pop());
			t[0].Flip();


			// screw DRY, copypasta is faster
			t = Tableaus[1];
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			while(temp.Count != 0)
				t.Add(temp.Pop());

			t = Tableaus[2];
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			while(temp.Count != 0)
				t.Add(temp.Pop());

			t = Tableaus[3];
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			while(temp.Count != 0)
				t.Add(temp.Pop());

			t = Tableaus[4];
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[0]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			while(temp.Count != 0)
				t.Add(temp.Pop());

			t = Tableaus[5];
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			while(temp.Count != 0)
				t.Add(temp.Pop());

			t = Tableaus[6];
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[1]].Pop());
			temp.Push(cards[Suits[2]].Pop());
			temp.Push(cards[Suits[2]].Pop());	// 2
			while(temp.Count != 0)
				t.Add(temp.Pop());

			//Stack<Card> flipped = new Stack<Card>();
			for(int i = 2; i < 4; i++) {
				foreach(Card card in cards[Suits[i]])
					Deck.Push(card);
			}
			//Deck = new Stack<Card>(flipped);
		}

		/// <summary>
		/// Deals a random game of Klondike.
		/// </summary>
		/// <param name="seed">Seed for RNG</param>
		public void Deal(int seed) {
			List<Card> allCards = new List<Card>();
			// generate deck
			foreach(Suit suit in Suits) {
				Foundations[suit] = new Stack<Card>();
				for(int i = 1; i < 14; i++) {
					allCards.Add(new Card(suit, i));
				}
			}

			// shuffle the deck
			// http://stackoverflow.com/a/1262619/776710
			Random rng = new Random(seed);
			int n = allCards.Count;
			while(n > 1) {
				n--;
				int k = rng.Next(n + 1);
				Card value = allCards[k];
				allCards[k] = allCards[n];
				allCards[n] = value;
			}

			Stack<Card> deck = new Stack<Card>(allCards);

			// put the cards on tableaus first
			for(int i = 0; i < NumTableaus; i++) {
				for(int j = i; j < NumTableaus; j++) {
					Card toAdd = deck.Pop();
					// the last card added to the tableau is face-up
					if(i == j)
						toAdd.Flip();
					Tableaus[j].Add(toAdd);
				}
			}

			// the rest of the cards go on the deck
			Deck = new Stack<Card>(deck);
		}

		/// <summary>
		/// Counts the number of cards in the foundations.
		/// </summary>
		/// <returns>The number of cards in the foundations</returns>
		public int FoundationOccupancy() {
			int total = 0;
			foreach(Suit suit in Suits)
				total += Foundations[suit].Count;
			return total;
		}

		/// <summary>
		/// Checks to see if all foundations are full.
		/// </summary>
		/// <returns>True if this state is a win state, false otherwise.</returns>
		public bool CheckForWin() {
			foreach(Suit suit in Suits) {
				if(Foundations[suit].Count != 13)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Pops the top [stack] cards from source, flips them, then pushes them to [dest].
		/// </summary>
		/// <param name="source">Stack of Cards to flip from</param>
		/// <param name="dest">Stack of Cards to flip to</param>
		/// <param name="count">Number of cards to flip. If count > source.Count, flip all.</param>
		private void FlipStacks(Stack<Card> source, Stack<Card> dest, int count) {
			for(int i = 0; i < Math.Min(count, source.Count); i++) {
				Card temp = source.Pop();
				temp.Flip();
				dest.Push(temp);
			}
		}

		/// <summary>
		/// Determines whether or not a move is legal in the current game state.
		/// </summary>
		/// <param name="move">Move to determine legality of</param>
		/// <returns>True if move is legal, false otherwise.</returns>
		public bool IsMoveLegal(Move move) {
			if(move.MoveType == Move.Type.Flip) {
				Card toFlip = Tableaus[(int)move.SourcePrimaryIndex].Last();
				if(toFlip.IsFaceUp)
					return false;
			}
			// ReverseIterateWaste is always illegal.
			if(move.MoveType == Move.Type.ReverseIterateWaste)
				return false;
			if(move.MoveType == Move.Type.Normal) {
				List<Card> toMove = new List<Card>();
				switch(move.SourceZone) {
					case Move.Zone.Tableaus:
						// out-of-bounds
						if((int)move.SourcePrimaryIndex >= NumTableaus)
							return false;
						List<Card> tableau = Tableaus[(int)move.SourcePrimaryIndex];
						if(tableau.Count <= move.SourceSecondaryIndex)
							return false;
							// if a card is not face-up, you can't move it.
						if(!tableau[move.SourceSecondaryIndex].IsFaceUp)
							return false;
						toMove = tableau.Skip(move.SourceSecondaryIndex).ToList<Card>();
						break;
					case Move.Zone.Foundations:
						if(!CanTakeFromFoundations)
							return false;
						Stack<Card> foundation = Foundations[(Suit)move.SourcePrimaryIndex];
						if(foundation.Count == 0)
							return false;
						toMove = new List<Card>();
						toMove.Add(foundation.Peek());
						break;
					case Move.Zone.Waste:
						if(Waste.Count == 0)
							return false;
						toMove = new List<Card>();
						toMove.Add(Waste.Peek());
						break;
				}

				switch(move.DestinationZone) {
					case Move.Zone.Tableaus:
						if((int)move.DestinationPrimaryIndex >= NumTableaus)
							return false;
						List<Card> tableau = Tableaus[(int)move.DestinationPrimaryIndex];
						if(tableau.Count == 0) {
							if(toMove[0].CardValue != 13)
								return false;
							break;
						}
						Card top = tableau.Last();
						if(top.CardSuit.Color == toMove[0].CardSuit.Color)
							return false;
						if(top.CardValue - 1 != toMove[0].CardValue)
							return false;
						break;
					case Move.Zone.Foundations:
						if(toMove.Count > 1)
							return false;
						Stack<Card> foundation = Foundations[toMove[0].CardSuit];
						if(foundation.Count == 0 && toMove[0].CardValue != 1)
							return false;
						if(foundation.Count != 0 && foundation.Peek().CardValue + 1 != toMove[0].CardValue)
							return false;
						break;
					case Move.Zone.Waste:
						return false;
				}

			}
			return true;
		}

		/// <summary>
		/// Executes a move on this state.
		/// </summary>
		/// <param name="move">The Move to be executed.</param>
		/// <param name="allowAllMoves">Whether otherwise illegal moves should be allowed, such as ReverseIterateWaste, used for undo</param>
		/// <exception cref="IllegalMoveException">If the move to be executed cannot legally be done</exception>
		public void ExecuteMove(Move move, bool allowAllMoves) {
			//if(!allowAllMoves && !IsMoveLegal(move))
			//	throw new IllegalMoveException("Tried to execute an illegal move when allowAllMoves was false.");
			switch(move.MoveType) {
				case Move.Type.Flip:
					// flip over the card in the tableau number SourcePrimaryIndex
					Card toFlip = Tableaus[(int)move.SourcePrimaryIndex].Last();
					if(toFlip.IsFaceUp)
						throw new IllegalMoveException("Cannot flip a card that is already face-up.");
					toFlip.Flip();
					break;

				case Move.Type.IterateWaste:
					if(Deck.Count == 0) {
						// deck is empty, so flip all cards in waste and shove em in the deck
						FlipStacks(Waste, Deck, Waste.Count);
					} else {
						// we want to move the top few cards of the deck to the top of the waste
						FlipStacks(Deck, Waste, DrawNumber);
					}
					break;

				case Move.Type.ReverseIterateWaste:
					throw new IllegalMoveException("ReverseIterateWaste is currently bugged; don't use it.");
					// not normally something you can do
					if(Waste.Count == 0) {
						// waste is empty, so flip all cards in the deck and shove em in the waste
						FlipStacks(Deck, Waste, Deck.Count);
					} else {
						// we want to move the top few cards of the waste to the top of the deck
						FlipStacks(Waste, Deck, DrawNumber);
					}
					break;

				case Move.Type.Normal:
					// first get the cards to move
					List<Card> toMove = new List<Card>();
					switch(move.SourceZone) {
						case Move.Zone.Tableaus:
							// you can't move a card that is face-down, even if you're undoing.
							// out-of-bounds
							if((int)move.SourcePrimaryIndex >= NumTableaus)
								throw new IllegalMoveException("Source tableau index is out of range.");
							List<Card> tableau = Tableaus[(int)move.SourcePrimaryIndex];
							if(tableau.Count <= move.SourceSecondaryIndex)
								throw new IllegalMoveException("Source card index is out of range.");
							if(!tableau[move.SourceSecondaryIndex].IsFaceUp)
								throw new IllegalMoveException("The source of a move cannot be face down");
							// you can move one or more cards from the tableau, provided they are face-up
							toMove = tableau.Skip(move.SourceSecondaryIndex).ToList<Card>();
							// remove those cards from the tableau
							tableau.RemoveRange(move.SourceSecondaryIndex, tableau.Count - move.SourceSecondaryIndex);
							break;
						case Move.Zone.Foundations:
							Stack<Card> foundation = Foundations[(Suit)move.SourcePrimaryIndex];
							if(foundation.Count == 0)
								throw new IllegalMoveException("The source foundation is empty.");
							toMove.Add(foundation.Pop());
							break;
						case Move.Zone.Waste:
							if(Waste.Count == 0)
								throw new IllegalMoveException("The waste pile is empty.");
							toMove.Add(Waste.Pop());
							break;
					}
					// then put them in the new location
					switch(move.DestinationZone) {
						case Move.Zone.Tableaus:
							if((int)move.DestinationPrimaryIndex >= NumTableaus)
								throw new IllegalMoveException("Destination tableau index is out of range.");
							List<Card> tableau = Tableaus[(int)move.DestinationPrimaryIndex];
							if(tableau.Count == 0) {
								if(toMove[0].CardValue != 13)
									throw new IllegalMoveException("Only a King (value 13) can be placed on an empty tableau.");
								tableau.AddRange(toMove);
							} else {
								Card top = tableau.Last();
								if(top.CardSuit.Color == toMove[0].CardSuit.Color)
									throw new IllegalMoveException("In order to put a card on top of another on the tableau, they must be different colors.");
								if(top.CardValue - 1 != toMove[0].CardValue)
									throw new IllegalMoveException("In order to put a card on top of another on the tableau, it must be one less than the card underneath it.");
								tableau.AddRange(toMove);
							}
							break;
						case Move.Zone.Foundations:
							if(toMove.Count > 1)
								throw new IllegalMoveException("Only one card at a time can be placed on the foundations.");
							Stack<Card> foundation = Foundations[toMove[0].CardSuit];
							if(foundation.Count == 0 && toMove[0].CardValue != 1)
								throw new IllegalMoveException("The ace (value 1) needs to be the first card on the foundations.");
							if(foundation.Count != 0 && foundation.Peek().CardValue + 1 != toMove[0].CardValue)
								throw new IllegalMoveException("In order to put a card on the foundation, it must be one more than the card underneath it.");
							foundation.Push(toMove[0]);
							break;
						case Move.Zone.Waste:
							if(toMove.Count > 1)
								throw new IllegalMoveException("Only one card at a time can be placed in the waste pile (and even then, only when undoing).");
							Waste.Push(toMove[0]);
							break;
					}
					break;
			}
			Moves.Enqueue(move);
		}

		/// <summary>
		/// Custom deep hash function
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() {
			Int64 temp = 0;
			int running = 1;
			Card[] cards = Deck.ToArray();
			foreach(Card card in cards)
				temp = (temp + card.GetHashCode() * running++) % int.MaxValue;

			foreach(List<Card> list in Tableaus) {
				cards = list.ToArray();
				foreach(Card card in cards)
					temp = (temp + card.GetHashCode() * running++) % int.MaxValue;
			}

			foreach(Suit suit in Suits) {
				cards = Foundations[suit].ToArray();
				foreach(Card card in cards)
					temp = (temp + card.GetHashCode() * running++) % int.MaxValue;
			}

			cards = Waste.ToArray();
			foreach(Card card in cards)
				temp = (temp + card.GetHashCode() * running++) % int.MaxValue;
			return (int)temp;
		}

		public override bool Equals(object other) {
			State o = (State)other;

			if(o.Waste.Count != Waste.Count || o.Deck.Count != Deck.Count)
				return false;
			// TODO: this is sloppy but im tired
			return GetHashCode() == o.GetHashCode();
		}
	}
}
