using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver.Objects {
	class Suit {
		public enum Colors { Red, Black };
		public Colors Color;
		public string Name;
		public char Symbol;

		public Suit(string name, Colors color, char symbol) {
			Name = name;
			Color = color;
			Symbol = symbol;
		}
	}
}
