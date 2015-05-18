using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver.Exceptions {
	/// <summary>
	/// Thrown when a move to be executed is not allowed.
	/// </summary>
	class IllegalMoveException : Exception {
		public IllegalMoveException() {

		}

		public IllegalMoveException(string message)
			: base(message) {

		}

		public IllegalMoveException(string message, Exception inner)
			: base(message, inner) {

		}
	}
}
