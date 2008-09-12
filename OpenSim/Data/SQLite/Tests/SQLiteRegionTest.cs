/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using OpenSim.Framework;
using OpenSim.Data.SQLite;
using OpenSim.Region.Environment.Scenes;
using OpenMetaverse;

namespace OpenSim.Data.SQLite.Tests
{
    [TestFixture]
    public class SQLiteRegionTest
    {
        public string file = "regiontest.db";
        public string connect;
        public SQLiteRegionData db;
        public UUID region = UUID.Zero;
        
        [TestFixtureSetUp]
        public void Init()
        {
            connect = "URI=file:" + file + ",version=3";
            db = new SQLiteRegionData();
            db.Initialise(connect);
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            System.IO.File.Delete(file);
        }
        
        [Test]
        public void T001_LoadEmpty()
        {
            List<SceneObjectGroup> objs = db.LoadObjects(region);
            Assert.AreEqual(0, objs.Count);
        }
        
        // SOG round trips
        //  * store objects, make sure they save
        //  * update 

        [Test]
        public void T010_StoreSimpleObject()
        {
            SceneObjectGroup sog = NewSOG("object1");
            SceneObjectGroup sog2 = NewSOG("object2");
            
            db.StoreObject(sog, region);
            db.StoreObject(sog2, region);

            // This tests the ADO.NET driver
            List<SceneObjectGroup> objs = db.LoadObjects(region);
            Assert.AreEqual(2, objs.Count);

            // This confirms things actually went to disk
            db = new SQLiteRegionData();
            db.Initialise(connect);

            List<SceneObjectGroup> objs2 = db.LoadObjects(region);
            Assert.AreEqual(2, objs2.Count);
        }
        
        [Test]
        public void T011_ObjectNames()
        {
            List<SceneObjectGroup> objs = db.LoadObjects(region);
            foreach (SceneObjectGroup sog in objs)
            {
                SceneObjectPart p = sog.RootPart;
                Assert.That("", Text.DoesNotMatch(p.Name));
                Assert.That(p.Name, Text.Matches(p.Description));
            }
        }

        [Test]
        public void T012_UpdateObject()
        {
            string text = "object1 text";
            SceneObjectGroup sog = FindSOG("object1", region);
            sog.RootPart.Text = text;
            db.StoreObject(sog, region);

            sog = FindSOG("object1", region);
            Assert.That(text, Text.Matches(sog.RootPart.Text));
        }

        // Extra private methods

        private SceneObjectGroup FindSOG(string name, UUID r)
        {
            List<SceneObjectGroup> objs = db.LoadObjects(r);
            foreach (SceneObjectGroup sog in objs)
            {
                SceneObjectPart p = sog.RootPart;
                if (p.Name == name) {
                    return sog;
                }
            }
            return null;
        }

        private SceneObjectGroup NewSOG(string name)
        {
            SceneObjectGroup sog = new SceneObjectGroup();
            SceneObjectPart sop = new SceneObjectPart();
            sop.LocalId = 1;
            sop.Name = name;
            sop.Description = name;
            sop.Text = "";
            sop.SitName = "";
            sop.TouchName = "";
            sop.UUID = UUID.Random();
            sop.Shape = PrimitiveBaseShape.Default;
            sog.AddPart(sop);
            sog.RootPart = sop;
            return sog;
        }

    }
}