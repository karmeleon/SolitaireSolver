using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver.Objects {
	class Card {
		public Suit CardSuit;
		public int CardValue;
		public bool IsFaceUp;

		/// <summary>
		/// Creates a face-down card with the given suit and value.
		/// </summary>
		/// <param name="suit">Suit of card to make.</param>
		/// <param name="value">Value of card to make (1 is Ace, 13 is King)</param>
		public Card(Suit suit, int value) {
			CardSuit = suit;
			CardValue = value;
			IsFaceUp = false;
		}

		/// <summary>
		/// Creates a card with the given suit, value, and facing direction.
		/// </summary>
		/// <param name="suit">Suit of card to make.</param>
		/// <param name="value">Value of card to make.</param>
		/// <param name="isFaceUp">Whether the card should be face-up.</param>
		public Card(Suit suit, int value, bool isFaceUp) {
			CardSuit = suit;
			CardValue = value;
			IsFaceUp = isFaceUp;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="other">Card to copy</param>
		public Card(Card other) {
			CardSuit = other.CardSuit;
			CardValue = other.CardValue;
			IsFaceUp = other.IsFaceUp;
		}

		/// <summary>
		/// Switches a card between face up and face down.
		/// </summary>
		public void Flip() {
			IsFaceUp = !IsFaceUp;
		}

		public override string ToString() {
			if(CardValue < 11)
				return "" + CardValue + CardSuit.Symbol;
			if(CardValue == 11)
				return "J" + CardSuit.Symbol;
			if(CardValue == 12)
				return "Q" + CardSuit.Symbol;
			if(CardValue == 13)
				return "K" + CardSuit.Symbol;
			return "IDK";
		}

		public override int GetHashCode() {
			Int64 temp = CardSuit.GetHashCode();
			temp += CardValue.GetHashCode();
			temp += IsFaceUp.GetHashCode();
			return (int)(temp % int.MaxValue);
		}
	}
}
