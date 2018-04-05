﻿//
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

using Heiflow.Core.Data;
using Heiflow.Models.Generic;
using Heiflow.Models.Generic.Attributes;
using Heiflow.Models.UI;
using ILNumerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heiflow.Models.Subsurface
{
    [PackageItem]
    [PackageCategory("Subsidence", true)]
    [CoverageItem]
    [Export(typeof(IMFPackage))]
    public class SUBPackage : MFPackage
    {
        public SUBPackage()
        {
            Name = "SUB";
            _FullName = "Subsidence Package";
            _PackageInfo.Format = FileFormat.Text;
            _PackageInfo.IOState = IOState.OLD;
            _PackageInfo.FileExtension = ".sub";
            _PackageInfo.ModuleName = "SUB";
            Description = "The Basic package is used to specify certain data used in all models. These include" +
                                        "\r\n1. the locations of active, inactive, and specified head cells," +
                                        "\r\n2. the head stored in inactive cells, and" +
                                        "\r\n3. the initial heads in all cells.";
            Version = "SUB1";
            IsMandatory = false;
            _Layer3DToken = "RegularGrid";
        }


        /// <summary>
        /// an array specifying the preconsolidation head or preconsolidation stress in terms of head in the aquifer for systems of no-delay interbeds
        /// </summary>
        [StaticVariableItem("Layer")]
        [Browsable(false)]
        [ArealProperty(typeof(float), 10)]
        public MyVarient3DMat<float> HC
        {
            get;
            set;
        }
        /// <summary>
        /// an array specifying the dimensionless elastic skeletal storage coefficient for systems of no-delay interbeds.
        /// </summary>
        [StaticVariableItem("Layer")]
        [Browsable(false)]
        [ArealProperty(typeof(float), 0.0001)]
        public MyVarient3DMat<float> Sfe
        {
            get;
            set;
        }
        /// <summary>
        ///  an array specifying the dimensionless inelastic skeletal storage coefficient for systems of no-delay interbeds
        /// </summary>
        [StaticVariableItem("Layer")]
        [Browsable(false)]
        [ArealProperty(typeof(float), 0.0001)]
        public MyVarient3DMat<float> Sfv
        {
            get;
            set;
        }
        /// <summary>
        /// an array specifying the starting compaction in each system of no-delay interbeds.
        /// </summary>
        [StaticVariableItem("Layer")]
        [Browsable(false)]
        [ArealProperty(typeof(float), 0.0001)]
        public MyVarient3DMat<float> Com
        {
            get;
            set;
        }
        public int [] OutputFIDs
        {
            get;
            set;
        }
        public override void Initialize()
        {
            this.Grid = Owner.Grid;
            this.Grid.Updated += this.OnGridUpdated;
            this.TimeService = Owner.TimeService;
            base.Initialize();
        }
        public override bool New()
        {
            base.New();
            return true;
        }
        /// <summary>
        ///  a flag used to control output of information generated by the Sub Package
        /// </summary>
        public int ISUBOC
        {
            get;
            set;
        }
        /// <summary>
        /// the number of systems of no-delay interbeds.
        /// </summary>
        public int NNDB 
        {
            get;
            set;
        }
        /// <summary>
        /// the number of systems of delay interbeds.
        /// </summary>
        public int NDB 
        {
            get;
            set;
        }
        /// <summary>
        /// the number of material zones that are needed to define the hydraulic properties of systems of delay interbeds. 
        /// Each material zone is defined by a combination of vertical hydraulic conductivity, 
        /// elastic specific storage, and inelastic specific storage.
        /// </summary>
        public int NMZ  
        {
            get;
            set;
        }
        /// <summary>
        ///  the number of nodes used to discretize the half space to approximate the head distributions in systems of delay interbeds.
        /// </summary>
        public int NN
        {
            get;
            set;
        }
        /// <summary>
        /// an acceleration parameter.  optimum values may range from 0.0 to 0.6.
        /// </summary>
        public float AC1 
        {
            get;
            set;
        }
        /// <summary>
        /// an acceleration parameter. Values are normally between 1.0 and 2.0, but the optimum is probably closer to 1.0 than to 2.0. 
        /// </summary>
        public float AC2
        {
            get;
            set;
        }
        /// <summary>
        /// the minimum number of iterations for which one-dimensional equations will be solved 
        /// for flow in interbeds when the Strongly Implicit Procedure (SIP) is used to solve the groundwater flow equations. 
        /// </summary>
        public int ITMIN 
        {
            get;
            set;
        }
        /// <summary>
        ///  a flag and a unit number.
        /// </summary>
        public int IDSAVE  
        {
            get;
            set;
        }
        /// <summary>
        /// a flag and a unit number.
        /// </summary>
        public int IDREST 
        {
            get;
            set;
        }
        public int[] LN
        {
            get;
            set;
        }
        public int[] LDN
        {
            get;
            set;
        }
        public override bool Load()
        {
            if (File.Exists(FileName))
            {
                StreamReader sr = new StreamReader(FileName);
                //Data Set 2: # ISUBCB ISUBOC NNDB NDB NMZ NN AC1 AC2 ITMIN IDSAVE IDREST SUBLNK
                string newline = ReadComment(sr);
                var buf = TypeConverterEx.Split<float>(newline,11);
                ISUBOC = (int)buf[1];
                NNDB = (int)buf[2];
                NDB = (int)buf[3];
                NMZ = (int)buf[4];
                NN = (int)buf[5];
                AC1 = buf[6];
                AC2= buf[7];
                ITMIN = (int)buf[8];
                IDSAVE = (int)buf[9];
                IDREST = (int)buf[10];

                if (NNDB > 0)
                {
                    newline = sr.ReadLine();
                    LN = TypeConverterEx.Split<int>(newline, NNDB);
                }
                if (NDB > 0)
                {
                    newline = sr.ReadLine();
                    LDN = TypeConverterEx.Split<int>(newline, NDB);
                }

                var grid = (Owner.Grid as MFGrid);

                if (NNDB > 0)
                {
                    this.HC = new MyVarient3DMat<float>(grid.ActualLayerCount, 1)
                    {
                        Name = "HC",
                        TimeBrowsable = false,
                        AllowTableEdit = true
                    };
                    this.Sfe = new MyVarient3DMat<float>(grid.ActualLayerCount, 1)
                    {
                        Name = "Sfe",
                        TimeBrowsable = false,
                        AllowTableEdit = true
                    };
                    this.Sfv = new MyVarient3DMat<float>(grid.ActualLayerCount, 1)
                    {
                        Name = "Sfv",
                        TimeBrowsable = false,
                        AllowTableEdit = true
                    };
                    this.Com = new MyVarient3DMat<float>(grid.ActualLayerCount, 1)
                    {
                        Name = "Com",
                        TimeBrowsable = false,
                        AllowTableEdit = true
                    };
                    for (int l = 0; l < grid.ActualLayerCount; l++)
                    {
                        this.HC.Variables[l] = "HC " + (l + 1);
                        ReadSerialArray(sr, this.HC, l, 0);

                        this.Sfe.Variables[l] = "Sfe " + (l + 1);
                        ReadSerialArray(sr, this.Sfe, l, 0);

                        this.Sfv.Variables[l] = "Sfv " + (l + 1);
                        ReadSerialArray(sr, this.Sfv, l, 0);

                        this.Com.Variables[l] = "Com " + (l + 1);
                        ReadSerialArray(sr, this.Com, l, 0);
                    }
                }

                newline = sr.ReadLine();
                OutputFIDs = TypeConverterEx.Split<int>(newline, 12);

                sr.Close();
                OnLoaded("successfully loaded");          
                return true;
            }
            else
            {
                Message = string.Format("\r\n Failed to load {0}. The package file does not exist: {1}", Name, FileName);
                OnLoadFailed(Message);
                return false;
            }
        }
        public override bool SaveAs(string filename,IProgress progress)
        {
            var grid = (Owner.Grid as IRegularGrid);
            StreamWriter sw = new StreamWriter(filename);
            WriteDefaultComment(sw, this.Name);
     
            OnSaved(progress);
            return true;
        }
        public override void Clear()
        {
            if (_Initialized)
                this.Grid.Updated -= this.OnGridUpdated;
            base.Clear();
        }

        public override void CompositeOutput(MFOutputPackage mfout)
        {
            var mf = Owner as Modflow;
            if (this.OutputFIDs != null && this.OutputFIDs[7] > 0)
            {
                var sfr_info = (from info in mf.NameManager.MasterList where info.FID == this.OutputFIDs[7] select info).First();
                // sfr_info.ModuleName = SFRPackage.PackageName;
                var sfr_out = new SubOutputPackage()
                {
                    Owner = mf,
                    PackageInfo = sfr_info,
                    FileName = sfr_info.FileName,
                    Parent = mfout
                };
                sfr_out.Initialize();
               // sfr_out.Scan();
            }
        }

        public override void Attach(DotSpatial.Controls.IMap map, string directory)
        {
            this.Feature = Owner.Grid.FeatureSet;
            this.FeatureLayer = Owner.Grid.FeatureLayer;
        }

        public override IPackage Extract(Modflow newmf)
        {
            return null;
        }
    }
}
