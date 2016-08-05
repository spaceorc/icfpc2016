using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.DiofantEquationSolver
{
    [TestFixture]
    class DiofantEquationSolver_Should
    {
        [Test]
        public void SolveSimpleTask()
        {
            var equation = new DiofantEquation(new Rational(1, 4), new Rational(1, 4), new Rational(1, 2));
            var result = equation.Solve(1);
            Assert.True(equation.Check(result));
        }


    }
}
