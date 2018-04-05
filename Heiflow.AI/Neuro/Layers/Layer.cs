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

namespace  Heiflow.AI.Neuro
{
    using System;

    /// <summary>
    /// Base neural layer class.
    /// </summary>
    /// 
    /// <remarks>This is a base neural layer class, which represents
    /// collection of neurons.</remarks>
    /// 
    [Serializable]
    public abstract class Layer
    {
        /// <summary>
        /// Layer's inputs count.
        /// </summary>
        protected int inputsCount = 0;

        /// <summary>
        /// Layer's neurons count.
        /// </summary>
        protected int neuronsCount = 0;

        /// <summary>
        /// Layer's neurons.
        /// </summary>
        protected Neuron[] neurons;

        /// <summary>
        /// Layer's output vector.
        /// </summary>
        protected double[] output;

        /// <summary>
        /// Layer's inputs count.
        /// </summary>
        public int InputsCount
        {
            get { return inputsCount; }
        }

        /// <summary>
        /// Layer's neurons count.
        /// </summary>
        public int NeuronsCount
        {
            get { return neuronsCount; }
        }

        /// <summary>
        /// Layer's output vector.
        /// </summary>
        /// 
        /// <remarks><para>The calculation way of layer's output vector is determined by neurons,
        /// which comprise the layer.</para>
        /// 
        /// <para><note>The property is not initialized (equals to <see langword="null"/>) until
        /// <see cref="Compute"/> method is called.</note></para>
        /// </remarks>
        /// 
        public double[] Output
        {
            get { return output; }
        }

        /// <summary>
        /// Layer's neurons accessor.
        /// </summary>
        /// 
        /// <param name="index">Neuron index.</param>
        /// 
        /// <remarks>Allows to access layer's neurons.</remarks>
        /// 
        public Neuron this[int index]
        {
            get { return neurons[index]; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer"/> class.
        /// </summary>
        /// 
        /// <param name="neuronsCount">Layer's neurons count.</param>
        /// <param name="inputsCount">Layer's inputs count.</param>
        /// 
        /// <remarks>Protected contructor, which initializes <see cref="inputsCount"/>,
        /// <see cref="neuronsCount"/> and <see cref="neurons"/> members.</remarks>
        /// 
        protected Layer( int neuronsCount, int inputsCount )
        {
            this.inputsCount = Math.Max( 1, inputsCount );
            this.neuronsCount = Math.Max( 1, neuronsCount );
            // create collection of neurons
            neurons = new Neuron[this.neuronsCount];
        }

        /// <summary>
        /// Compute output vector of the layer.
        /// </summary>
        /// 
        /// <param name="input">Input vector.</param>
        /// 
        /// <returns>Returns layer's output vector.</returns>
        /// 
        /// <remarks><para>The actual layer's output vector is determined by neurons,
        /// which comprise the layer - consists of output values of layer's neurons.
        /// The output vector is also stored in <see cref="Output"/> property.</para>
        /// 
        /// <para><note>The method may be called safely from multiple threads to compute layer's
        /// output value for the specified input values. However, the value of
        /// <see cref="Output"/> property in multi-threaded environment is not predictable,
        /// since it may hold layer's output computed from any of the caller threads. Multi-threaded
        /// access to the method is useful in those cases when it is required to improve performance
        /// by utilizing several threads and the computation is based on the immediate return value
        /// of the method, but not on layer's output property.</note></para>
        /// </remarks>
        /// 
        public virtual double[] Compute( double[] input )
        {
            // local variable to avoid mutlithread conflicts
            double[] output = new double[neuronsCount];

            // compute each neuron
            for ( int i = 0; i < neuronsCount; i++ )
                output[i] = neurons[i].Compute( input );

            // assign output property as well (works correctly for single threaded usage)
            this.output = output;

            return output;
        }

        /// <summary>
        /// Randomize neurons of the layer.
        /// </summary>
        /// 
        /// <remarks>Randomizes layer's neurons by calling <see cref="Neuron.Randomize"/> method
        /// of each neuron.</remarks>
        /// 
        public virtual void Randomize( )
        {
            foreach ( Neuron neuron in neurons )
                neuron.Randomize( );
        }
    }
}
