﻿using NBi.Core.Xml;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NBi.Testing.Unit.Core.Xml
{
    public class XPathStreamEngineTest
    {
        private class XPathStreamEngine : XPathEngine
        {
            private readonly StreamReader streamReader;

            public XPathStreamEngine(StreamReader streamReader, string from, IEnumerable<ElementSelect> selects)
                : base(from,selects)
            {
                this.streamReader=streamReader;
            }

            public override NBi.Core.ResultSet.ResultSet Execute()
            {
                var doc = XDocument.Load(streamReader);
                return Execute(doc);
            }
        }

        
        protected StreamReader GetResourceReader()
        {
            // A Stream is needed to read the XML document.
            var stream = Assembly.GetExecutingAssembly()
                                           .GetManifestResourceStream("NBi.Testing.Unit.Core.Resources.PurchaseOrders.xml");
            var reader = new StreamReader(stream);
            return reader;
        }

        [Test]
        public void Execute_Example_ColumnCount()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new ElementSelect("//PurchaseOrder/PurchaseOrderNumber")
                , new AttributeSelect(".", "PartNumber")
                , new ElementSelect("//PurchaseOrder/Address[@Type=\"Shiping\"]/City")
            };
            
            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Columns.Count, Is.EqualTo(3));
            }
        }

        [Test]
        [TestCase("//PurchaseOrder/Items/Item", 5)]
        [TestCase("//PurchaseOrder", 3)]
        public void Execute_Example_RowCount(string from, int rowCount)
        {
            var selects = new List<ElementSelect>()
            {
                new ElementSelect("//PurchaseOrder/PurchaseOrderNumber")
                , new AttributeSelect(".","PartNumber")
                , new ElementSelect("//PurchaseOrder/Address[@Type=\"Shiping\"]/City")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows.Count, Is.EqualTo(rowCount));
            }

            
        }

        [Test]
        public void Execute_FromElement_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items/Item/ProductName";
            var selects = new List<ElementSelect>()
            {
                new ElementSelect(".")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo("Lawnmower"));
            }
        }

        [Test]
        public void Execute_FromAttribute_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new AttributeSelect(".","PartNumber")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo("872-AA"));
            }
        }

        [Test]
        public void Execute_ChildElement_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new ElementSelect("//PurchaseOrder/Items/Item/ProductName")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo("Lawnmower"));
            }
        }

        [Test]
        public void Execute_ChildAttribute_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items";
            var selects = new List<ElementSelect>()
            {
                new AttributeSelect("//PurchaseOrder/Items/Item","PartNumber")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo("872-AA"));
            }
        }

        [Test]
        public void Execute_ParentElement_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new ElementSelect("//PurchaseOrder")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.StringStarting("Ellen Adams"));
                Assert.That(result.Rows[0].ItemArray[0], Is.StringContaining("Maple Street"));
            }
        }

        [Test]
        public void Execute_ParentAttribute_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new AttributeSelect("//PurchaseOrder","PurchaseOrderNumber")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo("99503"));
            }
        }

        [Test]
        public void Execute_MissingElement_Null()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new ElementSelect("//PurchaseOrder/Missing")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo(DBNull.Value));
            }
        }

        [Test]
        public void Execute_MissingAttribute_ValueCorrect()
        {
            var from = "//PurchaseOrder/Items/Item";
            var selects = new List<ElementSelect>()
            {
                new AttributeSelect("//PurchaseOrder", "Missing")
            };

            using (var reader = GetResourceReader())
            {
                var engine = new XPathStreamEngine(reader, from, selects);
                var result = engine.Execute();
                Assert.That(result.Rows[0].ItemArray[0], Is.EqualTo(DBNull.Value));
            }
        }
    }
}
