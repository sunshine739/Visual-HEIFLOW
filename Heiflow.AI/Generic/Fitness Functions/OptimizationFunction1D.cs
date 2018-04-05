//
// The Visual HEIFLOW License
//
// Copyright (c) 2015-2018 Yong Tian, SUSTech, Shenzhen, China. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Note: only part of the files distributed in the software belong to the Visual HEIFLOW. 
// The software also contains contributed files, which may have their own copyright notices.
//  If not, the GNU General Public License holds for them, too, but so that the author(s) 
// of the file have the Copyright.

namespace  Heiflow.AI.Genetic
{
    using System;
    using  Heiflow.AI;

    /// <summary>Base class for one dimensional function optimizations.</summary>
    /// 
    /// <remarks><para>The class is aimed to be used for one dimensional function
    /// optimization problems. It implements all methods of <see cref="IFitnessFunction"/>
    /// interface and requires overriding only one method -
    /// <see cref="OptimizationFunction"/>, which represents the
    /// function to optimize.</para>
    /// 
    /// <para><note>The optimization function should be greater
    /// than 0 on the specified optimization range.</note></para>
    /// 
    /// <para>The class works only with binary chromosomes (<see cref="BinaryChromosome"/>).</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // define optimization function
    /// public class UserFunction : OptimizationFunction1D
    /// {
    ///	    public UserFunction( ) :
    ///         base( new DoubleRange( 0, 255 ) ) { }
    ///
    /// 	public override double OptimizationFunction( double x )
    ///		{
    ///			return Math.Cos( x / 23 ) * Math.Sin( x / 50 ) + 2;
    ///		}
    /// }
    /// ...
    /// // create genetic population
    /// Population population = new Population( 40,
    ///		new BinaryChromosome( 32 ),
    ///		new UserFunction( ),
    ///		new EliteSelection( ) );
    ///	
    /// while ( true )
    /// {
    ///	    // run one epoch of the population
    ///     population.RunEpoch( );
    ///     // ...
    /// }
    /// </code>
    /// </remarks>
    ///
    public abstract class OptimizationFunction1D : IFitnessFunction
    {
        /// <summary>
        /// Optimization modes.
        /// </summary>
        ///
        /// <remarks>The enumeration defines optimization modes for
        /// the one dimensional function optimization.</remarks> 
        ///
        public enum Modes
        {
            /// <summary>
            /// Search for function's maximum value.
            /// </summary>
            Maximization,
            /// <summary>
            /// Search for function's minimum value.
            /// </summary>
            Minimization
        }

        // optimization range
        private DoubleRange	range = new DoubleRange( 0, 1 );
        // optimization mode
        private Modes mode = Modes.Maximization;

        /// <summary>
        /// Optimization range.
        /// </summary>
        /// 
        /// <remarks>Defines function's input range. The function's extreme point will
        /// be searched in this range only.
        /// </remarks>
        /// 
        public DoubleRange Range
        {
            get { return range; }
            set { range = value; }
        }

        /// <summary>
        /// Optimization mode.
        /// </summary>
        ///
        /// <remarks>Defines optimization mode - what kind of extreme point to search.</remarks> 
        ///
        public Modes Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptimizationFunction1D"/> class.
        /// </summary>
        ///
        /// <param name="range">Specifies range for optimization.</param>
        ///
        public OptimizationFunction1D( DoubleRange range )
        {
            this.range = range;
        }

        /// <summary>
        /// Evaluates chromosome.
        /// </summary>
        /// 
        /// <param name="chromosome">Chromosome to evaluate.</param>
        /// 
        /// <returns>Returns chromosome's fitness value.</returns>
        ///
        /// <remarks>The method calculates fitness value of the specified
        /// chromosome.</remarks>
        ///
        public double Evaluate( IChromosome chromosome )
        {
            double functionValue = OptimizationFunction( Translate( chromosome ) );
            // fitness value
            return ( mode == Modes.Maximization ) ? functionValue : 1 / functionValue;
        }

        /// <summary>
        /// Translates genotype to phenotype.
        /// </summary>
        /// 
        /// <param name="chromosome">Chromosome, which genoteype should be
        /// translated to phenotype.</param>
        ///
        /// <returns>Returns chromosome's fenotype - the actual solution
        /// encoded by the chromosome.</returns> 
        /// 
        /// <remarks>The method returns double value, which represents function's
        /// input point encoded by the specified chromosome.</remarks>
        ///
        public double Translate( IChromosome chromosome )
        {
            // get chromosome's value and max value
            double val = ( (BinaryChromosome) chromosome ).Value;
            double max = ( (BinaryChromosome) chromosome ).MaxValue;

            // translate to optimization's funtion space
            return val * range.Length / max + range.Min;
        }

        /// <summary>
        /// Function to optimize.
        /// </summary>
        ///
        /// <param name="x">Function's input value.</param>
        /// 
        /// <returns>Returns function output value.</returns>
        /// 
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        ///
        public abstract double OptimizationFunction( double x );
    }
}
