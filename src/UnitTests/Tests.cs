//******************************************************************************************************
//  Tests.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/04/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gemstone.COMTRADE.UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void FunctionalityTests()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test()
        {
            var path = @"E:\MohammadHadi\Documents\ESFA\Hafez\Resource\SettingsAndRelayFiles\a.cfg";
            var schema = new Schema(path);
            var parser = new Parser();
            parser.Schema = schema;

            parser.OpenFiles();
            var startTime = schema.StartTime;
            var triggerTime = schema.TriggerTime;
            var lineFrequency = schema.NominalFrequency;
            var samplingFrequency = schema.SampleRates[0].Rate;

            var analogSignals = new List<double[]>();
            var digitalSignals = new List<byte[]>();
            var sampleCount = parser.Schema.TotalSamples;

            for (int i = 0; i < parser.Schema.TotalAnalogChannels; i++)
            {
                analogSignals.Add(new double[sampleCount]);
            }

            for (int i = 0; i < parser.Schema.TotalDigitalChannels; i++)
            {
                digitalSignals.Add(new byte[sampleCount]);
            }

            var sampleNumber = 0;

            while (parser.ReadNext())
            {
                for (int i = 0; i < analogSignals.Count; i++)
                {
                    analogSignals[i][sampleNumber] = parser.Values[i];
                }

                for (int i = 0; i < digitalSignals.Count; i++)
                {
                    digitalSignals[i][sampleNumber] = (byte)parser.Values[analogSignals.Count + i];
                }

                sampleNumber++;
            } 

        }
    }
}
