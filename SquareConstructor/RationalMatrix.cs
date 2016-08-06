using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace SquareConstructor
{
	class TransposeOperator
	{
		private Rational[,] Matrix = new Rational[2,2];
		public Vector MoveVector = new Vector();

		public Rational this[int x, int y]
		{
			get { return Matrix[x, y]; }
			set { Matrix[x, y] = value; }
		}
		
		public Vector Apply(Vector vector)
		{
			return new Vector(vector.X*Matrix[0, 0] + vector.Y*Matrix[0, 1], vector.X*Matrix[1, 0] + vector.Y*Matrix[1, 1]) +
				   MoveVector;
		}

		public static TransposeOperator ConstructOperator(Segment source, Segment destination)
		{
			var sVector = source.End - source.Start;
			var dVector = destination.End - destination.Start;

			var cos = GeometryExtensions.GetCos(sVector, dVector);
			var sin = GeometryExtensions.GetSin(sVector, dVector);

			var matrix = new TransposeOperator();
			matrix[0, 0] = matrix[1, 1] = cos;
			matrix[1, 0] = sin;
			matrix[0, 1] = -sin;

			var newDVector = matrix.Apply(source.Start);
			matrix.MoveVector = destination.Start - newDVector;
			return matrix;
		}
	}
}
