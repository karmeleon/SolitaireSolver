using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver.Objects {
	class Move {
		public enum Zone { Tableaus, Foundations, Waste };
		// Normal for moving cards/stacks around, IterateWaste to pull new cards from the deck to the waste/move all waste to deck
		// ReverseIterateWaste to undo an IterateWaste
		// Flip to flip a card after uncovering it
		public enum Type { Normal, Flip, IterateWaste, ReverseIterateWaste }

		public Type MoveType;

		public Zone SourceZone;
		// Suit to tell which foundation is being referenced, int for Tableaus
		public object SourcePrimaryIndex;
		// index from bottom of tableau of the card to move
		public int SourceSecondaryIndex;

		public Zone DestinationZone;
		// destinationPrimaryIndex is only needed if the destination zone is the tableaus or if you want reversing to work.
		public object DestinationPrimaryIndex;
		// there is no DestinationSecondaryIndex because cards can only be added to the top of a tableau

		/// <summary>
		/// Creates a new Move of the given type.
		/// </summary>
		/// <param name="type">Type of move to make</param>
		public Move(Type type) {
			MoveType = type;
		}

		/// <summary>
		/// Creates a new flip move.
		/// </summary>
		/// <param name="sourcePrimaryIndex">Number of the tableau to flip the top card of.</param>
		public Move(int sourcePrimaryIndex) {
			MoveType = Type.Flip;
			SourcePrimaryIndex = sourcePrimaryIndex;
		}

		/// <summary>
		/// Creates a normal Move from the given source to the given destination
		/// </summary>
		/// <param name="sourceZone"></param>
		/// <param name="sourcePrimaryIndex"></param>
		/// <param name="sourceSecondaryIndex"></param>
		/// <param name="destinationZone"></param>
		/// <param name="destinationPrimaryIndex"></param>
		public Move(Zone sourceZone, object sourcePrimaryIndex, int sourceSecondaryIndex, Zone destinationZone, object destinationPrimaryIndex) {
			MoveType = Type.Normal;
			SourceZone = sourceZone;
			SourcePrimaryIndex = sourcePrimaryIndex;
			SourceSecondaryIndex = sourceSecondaryIndex;
			DestinationZone = destinationZone;
			DestinationPrimaryIndex = destinationPrimaryIndex;
		}

		/// <summary>
		/// Copy constructor for Move
		/// </summary>
		/// <param name="other">Move to copy</param>
		public Move(Move other) {
			MoveType = other.MoveType;
			SourceZone = other.SourceZone;
			SourcePrimaryIndex = other.SourcePrimaryIndex;
			SourceSecondaryIndex = other.SourceSecondaryIndex;
			DestinationZone = other.DestinationZone;
			DestinationPrimaryIndex = other.DestinationPrimaryIndex;
		}

		/// <summary>
		/// Reverses the source and destination of this move. Note that the result may not be a legal move.
		/// TODO: reversing a move that moves a stack causes disaster
		/// TODO: reversing an IterateWaste properly requires knowledge of the number of cards actually flipped
		/// </summary>
		public void Reverse() {
			if(MoveType == Type.IterateWaste)
				// TODO: figure out how reversing a move would work when less than the full number
				// of cards were iterated through on the IterateWaste move
				MoveType = Type.ReverseIterateWaste;
			else if(MoveType == Type.ReverseIterateWaste)
				MoveType = Type.IterateWaste;
			else {
				Zone tempZone = SourceZone;
				object tempPrimary = SourcePrimaryIndex;

				SourceZone = DestinationZone;
				SourcePrimaryIndex = DestinationPrimaryIndex;
				// TODO: figure out how reversing a move would work when moving a stack
				// you would have to know the size of the stack of cards being moved
				//SourceSecondaryIndex = DestinationSecondaryIndex;

				DestinationZone = tempZone;
				DestinationPrimaryIndex = tempPrimary;
			}
		}

		public override string ToString() {
			if(MoveType == Type.IterateWaste)
				return "Iterate through the waste pile.";
			if(MoveType == Type.ReverseIterateWaste)
				return "Reverse iterate through the waste pile.";
			if(MoveType == Type.Flip)
				return "Flip the top card in tableau " + SourcePrimaryIndex;

			string text = "";
			if(SourceZone == Zone.Waste)
				text = "Move the top card from the waste pile to ";
			else if(SourceZone == Zone.Foundations)
				text = "Move the top card from the " + Enum.GetName(typeof(Suit), (Suit)SourcePrimaryIndex) + " foundation to ";
			else if(SourceZone == Zone.Tableaus)
				text = "Move the stack of cards starting " + SourceSecondaryIndex + " cards from the bottom of tableau " + (int)SourcePrimaryIndex + " to ";

			if(DestinationZone == Zone.Foundations)
				text += "the top of the proper foundation.";
			else if(DestinationZone == Zone.Tableaus)
				text += "the top of tableau " + (int)DestinationPrimaryIndex + ".";

			return text;
		}
	}
}
