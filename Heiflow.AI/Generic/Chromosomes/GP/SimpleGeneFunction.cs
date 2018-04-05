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

    /// <summary>
    /// Genetic programming gene, which represents simple arithmetic functions and arguments.
    /// </summary>
    ///
    /// <remarks><para>Simple gene function may represent an arithmetic function (+, -, *, /) or
    /// an argument to function. This class is used by Genetic Programming (or Gene Expression Programming)
    /// chromosomes to build arbitrary expressions with help of genetic operators.</para>
    /// </remarks>
    ///
    public class SimpleGeneFunction : IGPGene
    {
        /// <summary>
        /// Enumeration of supported functions.
        /// </summary>
        protected enum Functions
        {
            /// <summary>
            /// Addition operator.
            /// </summary>
            Add,
            /// <summary>
            /// Suntraction operator.
            /// </summary>
            Subtract,
            /// <summary>
            /// Multiplication operator.
            /// </summary>
            Multiply,
            /// <summary>
            /// Division operator.
            /// </summary>
            Divide,
        }

        /// <summary>
        /// Number of different functions supported by the class.
        /// </summary>
        protected const int FunctionsCount = 4;

        // gene type
        private GPGeneType	type;
        // total amount of variables in the task which is supposed to be solved
        private int variablesCount;
        //
        private int val;

        /// <summary>
        /// Random number generator for chromosoms generation.
        /// </summary>
        protected static Random	rand = new Random( );

        /// <summary>
        /// Gene type.
        /// </summary>
        /// 
        /// <remarks><para>The property represents type of a gene - function, argument, etc.</para>
        /// </remarks>
        /// 
        public GPGeneType GeneType
        {
            get { return type; }
        }

        /// <summary>
        /// Arguments count.
        /// </summary>
        /// 
        /// <remarks><para>Arguments count of a particular function gene.</para></remarks>
        /// 
        public int ArgumentsCount
        {
            get { return ( type == GPGeneType.Argument ) ? 0 : 2; }
        }

        /// <summary>
        /// Maximum arguments count.
        /// </summary>
        /// 
        /// <remarks><para>Maximum arguments count of a function gene supported by the class.
        /// The property may be used by chromosomes' classes to allocate correctly memory for
        /// functions' arguments, for example.</para></remarks>
        /// 
        public int MaxArgumentsCount
        {
            get { return 2; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleGeneFunction"/> class.
        /// </summary>
        /// 
        /// <param name="variablesCount">Total amount of variables in the task which is supposed
        /// to be solved.</param>
        /// 
        /// <remarks><para>The constructor creates randomly initialized gene with random type
        /// and value by calling <see cref="Generate( )"/> method.</para></remarks>
        /// 
        public SimpleGeneFunction( int variablesCount ) : this( variablesCount, true ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleGeneFunction"/> class.
        /// </summary>
        /// 
        /// <param name="variablesCount">Total amount of variables in the task which is supposed
        /// to be solved.</param>
        /// <param name="type">Gene type to set.</param>
        /// 
        /// <remarks><para>The constructor creates randomly initialized gene with random
        /// value and preset gene type.</para></remarks>
        /// 
        public SimpleGeneFunction( int variablesCount, GPGeneType type )
        {
            this.variablesCount = variablesCount;
            // generate the gene value
            Generate( type );
        }

        // Private constructor
        private SimpleGeneFunction( int variablesCount, bool random )
        {
            this.variablesCount = variablesCount;
            // generate the gene value
            if ( random )
                Generate( );
        }

        /// <summary>
        /// Get string representation of the gene.
        /// </summary>
        /// 
        /// <returns>Returns string representation of the gene.</returns>
        /// 
        public override string ToString( )
        {
            if ( type == GPGeneType.Function )
            {
                // get function string representation
                switch ( (Functions) val )
                {
                    case Functions.Add:			// addition
                        return "+";

                    case Functions.Subtract:	// subtraction
                        return "-";

                    case Functions.Multiply:	// multiplication
                        return "*";

                    case Functions.Divide:		// division
                        return "/";
                }
            }

            // get argument string representation
            return string.Format( "${0}", val );
        }

        /// <summary>
        /// Clone the gene.
        /// </summary>
        /// 
        /// <remarks><para>The method clones the chromosome returning the exact copy of it.</para></remarks>
        /// 
        public IGPGene Clone( )
        {
            // create new gene ...
            SimpleGeneFunction clone = new SimpleGeneFunction( variablesCount, false );
            // ... with the same type and value
            clone.type = type;
            clone.val = val;

            return clone;
        }

        /// <summary>
        /// Randomize gene with random type and value.
        /// </summary>
        /// 
        /// <remarks><para>The method randomizes the gene, setting its type and value randomly.</para></remarks>
        /// 
        public void Generate( )
        {
            // give more chance to function
            Generate( ( rand.Next( 4 ) == 3 ) ? GPGeneType.Argument : GPGeneType.Function );
        }

        /// <summary>
        /// Randomize gene with random value.
        /// </summary>
        /// 
        /// <param name="type">Gene type to set.</param>
        /// 
        /// <remarks><para>The method randomizes a gene, setting its value randomly, but type
        /// is set to the specified one.</para></remarks>
        ///
        public void Generate( GPGeneType type )
        {
            // gene type
            this.type = type;
            // gene value
            val = rand.Next( ( type == GPGeneType.Function ) ? FunctionsCount : variablesCount );

        }

        /// <summary>
        /// Creates new gene with random type and value.
        /// </summary>
        /// 
        /// <remarks><para>The method creates new randomly initialized gene .
        /// The method is useful as factory method for those classes, which work with gene's interface,
        /// but not with particular gene class.</para>
        /// </remarks>
        /// 
        public IGPGene CreateNew( )
        {
            return new SimpleGeneFunction( variablesCount );
        }

        /// <summary>
        /// Creates new gene with certain type and random value.
        /// </summary>
        /// 
        /// <param name="type">Gene type to create.</param>
        /// 
        /// <remarks><para>The method creates new gene with specified type, but random value.
        /// The method is useful as factory method for those classes, which work with gene's interface,
        /// but not with particular gene class.</para>
        /// </remarks>
        /// 
        public IGPGene CreateNew( GPGeneType type )
        {
            return new SimpleGeneFunction( variablesCount, type );
        }
    }
}
