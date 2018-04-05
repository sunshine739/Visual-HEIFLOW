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

namespace  Heiflow.AI
{
    using System;

    /// <summary>
    /// Represents a double range with minimum and maximum values.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>The class represents a double range with inclusive limits -
    /// both minimum and maximum values of the range are included into it.
    /// Mathematical notation of such range is <b>[min, max]</b>.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create [0.25, 1.5] range
    /// DoubleRange range1 = new DoubleRange( 0.25, 1.5 );
    /// // create [1.00, 2.25] range
    /// DoubleRange range2 = new DoubleRange( 1.00, 2.25 );
    /// // check if values is inside of the first range
    /// if ( range1.IsInside( 0.75 ) )
    /// {
    ///     // ...
    /// }
    /// // check if the second range is inside of the first range
    /// if ( range1.IsInside( range2 ) )
    /// {
    ///     // ...
    /// }
    /// // check if two ranges overlap
    /// if ( range1.IsOverlapping( range2 ) )
    /// {
    ///     // ...
    /// }
    /// </code>
    /// </remarks>
    /// 
    public class DoubleRange
    {
        private double min, max;

        /// <summary>
        /// Minimum value of the range.
        /// </summary>
        /// 
        /// <remarks><para>The property represents minimum value (left side limit) or the range -
        /// [<b>min</b>, max].</para></remarks>
        /// 
        public double Min
        {
            get { return min; }
            set { min = value; }
        }

        /// <summary>
        /// Maximum value of the range.
        /// </summary>
        /// 
        /// <remarks><para>The property represents maximum value (right side limit) or the range -
        /// [min, <b>max</b>].</para></remarks>
        /// 
        public double Max
        {
            get { return max; }
            set { max = value; }
        }

        /// <summary>
        /// Length of the range (deffirence between maximum and minimum values).
        /// </summary>
        public double Length
        {
            get { return max - min; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleRange"/> class.
        /// </summary>
        /// 
        /// <param name="min">Minimum value of the range.</param>
        /// <param name="max">Maximum value of the range.</param>
        /// 
        public DoubleRange( double min, double max )
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Check if the specified value is inside of the range.
        /// </summary>
        /// 
        /// <param name="x">Value to check.</param>
        /// 
        /// <returns><b>True</b> if the specified value is inside of the range or
        /// <b>false</b> otherwise.</returns>
        /// 
        public bool IsInside( double x )
        {
            return ( ( x >= min ) && ( x <= max ) );
        }

        /// <summary>
        /// Check if the specified range is inside of the range.
        /// </summary>
        /// 
        /// <param name="range">Range to check.</param>
        /// 
        /// <returns><b>True</b> if the specified range is inside of the range or
        /// <b>false</b> otherwise.</returns>
        /// 
        public bool IsInside( DoubleRange range )
        {
            return ( ( IsInside( range.min ) ) && ( IsInside( range.max ) ) );
        }

        /// <summary>
        /// Check if the specified range overlaps with the range.
        /// </summary>
        /// 
        /// <param name="range">Range to check for overlapping.</param>
        /// 
        /// <returns><b>True</b> if the specified range overlaps with the range or
        /// <b>false</b> otherwise.</returns>
        /// 
        public bool IsOverlapping( DoubleRange range )
        {
            return ( ( IsInside( range.min ) ) || ( IsInside( range.max ) ) ||
                     ( range.IsInside( min ) ) || ( range.IsInside( max ) ) );
        }
    }
}
