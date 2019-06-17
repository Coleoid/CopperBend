//using System;
//using System.Collections.Generic;
//using NUnit.Framework;

//namespace Newtonsoft.Json.Tests.Issues
//{
//    [TestFixture]
//    public class IssueNNNN
//    {
//        //  All failing classes have:
//        //      a 0-param constructor
//        //      a property with a complex setter
//        //      the property has a generic in its type

//        //  If we hide the 0-param ctor with a 1-param ctor, deserialization works.
//        //  If the property simply assigns its value to an internal field, deserialization works.
//        //  If the property type is string or string[], deserialization works.
//        [Test]
//        public void Test()
//        {
//            var failing = new Failing_Two();

//            failing.AddItem("Fun", "(2,2)");
//            failing.AddItem("Games", "(7,7)");

//            //  Note that even with a failing object, the serialization is correct.
//            //  Passing_(One|Two|Three) and Failing_(One|Two|Three) objects serialize to identical JSON.
//            var jsonFailing = JsonConvert.SerializeObject(failing);

//            //  It's the deserialization which loses the data.
//            var newFailing = JsonConvert.DeserializeObject<Failing_Two>(jsonFailing);

//            Assert.AreEqual("Fun", newFailing.BackingStore["(2,2)"]);
//            Assert.AreEqual("Games", newFailing.BackingStore["(7,7)"]);
//        }

//        [TestCase("Passing_One")]
//        [TestCase("Passing_Two")]
//        [TestCase("Passing_Three")]
//        [TestCase("Failing_One")]
//        [TestCase("Failing_Two")]
//        [TestCase("Failing_Three")]
//        public void Test_six(string whichObject)
//        {
//            //  Note that even with a failing object, the serialization is correct.
//            //  Passing_(One|Two|Three) and Failing_(One|Two|Three) objects serialize to identical JSON.
//            IShowIssueNNNN tryMe = new Passing_Three();

//            tryMe.AddItem("Fun", "(2,2)");
//            tryMe.AddItem("Games", "(7,7)");

//            var json = JsonConvert.SerializeObject(tryMe);

//            IShowIssueNNNN dserTry = null;
//            switch (whichObject)
//            {
//            case "Passing_One":
//                dserTry = JsonConvert.DeserializeObject<Passing_One>(json);
//                break;
//            case "Passing_Two":
//                dserTry = JsonConvert.DeserializeObject<Passing_Two>(json);
//                break;
//            case "Passing_Three":
//                dserTry = JsonConvert.DeserializeObject<Passing_Three>(json);
//                break;
//            case "Failing_One":
//                dserTry = JsonConvert.DeserializeObject<Failing_One>(json);
//                break;
//            case "Failing_Two":
//                dserTry = JsonConvert.DeserializeObject<Failing_Two>(json);
//                break;
//            case "Failing_Three":
//                dserTry = JsonConvert.DeserializeObject<Failing_Three>(json);
//                break;
//            default:
//                throw new Exception("This unit test has a bug.");
//            }

//            Assert.AreEqual("Fun", dserTry.BackingStore["(2,2)"]);
//            Assert.AreEqual("Games", dserTry.BackingStore["(7,7)"]);
//        }

//        public interface IShowIssueNNNN
//        {
//            Dictionary<string, string> BackingStore { get; set; }
//            Dictionary<string, string> SerialItems { get; set; }
//            void AddItem(string item, string position);
//        }


//        public class Failing_One : IShowIssueNNNN
//        {
//            //  In the original code, the backing store is a different sort of object,
//            // from a third party library, and it isn't serializable.
//            //  My app only touches BackingStore, and Json.NET only touches SerialItems.
//            [JsonIgnore]
//            public Dictionary<string, string> BackingStore { get; set; } = new Dictionary<string, string>();

//            //  The getter and setter below contain the adapter code.
//            //  Originally, the dictionary type, classes, and adapter logic were more complex.  :D
//            public Dictionary<string, string> SerialItems
//            {
//                get
//                {
//                    var items = new Dictionary<string, string>();
//                    foreach (var key in BackingStore.Keys)
//                    {
//                        items[key] = BackingStore[key];
//                    }

//                    return items;
//                }

//                set
//                {
//                    // fails (this setter is not called during deserialization):
//                    BackingStore = new Dictionary<string, string>();
//                    foreach (var key in value.Keys)
//                    {
//                        BackingStore[key] = value[key];
//                    }
//                }
//            }

//            public void AddItem(string item, string position)
//            {
//                BackingStore[position] = item;
//            }
//        }

//        public class Passing_One : IShowIssueNNNN
//        {
//            //  A copy/paste of Failing_One.
//            //  The only differences between this class and the failing class above:
//            //  * Different class name
//            //  * Different comments
//            //  * This ctor:
//            public Passing_One(int dummy)
//            { }

//            [JsonIgnore]
//            public Dictionary<string, string> BackingStore { get; set; } = new Dictionary<string, string>();

//            public Dictionary<string, string> SerialItems
//            {
//                get
//                {
//                    var items = new Dictionary<string, string>();
//                    foreach (var key in BackingStore.Keys)
//                    {
//                        items[key] = BackingStore[key];
//                    }

//                    return items;
//                }

//                set
//                {
//                    // now being called during deserialization, and succeeding:
//                    BackingStore = new Dictionary<string, string>();
//                    foreach (var key in value.Keys)
//                    {
//                        BackingStore[key] = value[key];
//                    }
//                }
//            }

//            public void AddItem(string item, string position)
//            {
//                BackingStore[position] = item;
//            }
//        }

//        //  The issue doesn't involve inheritance, part one:
//        //  Child of a failing class fails.
//        public class Failing_Two : Failing_One, IShowIssueNNNN
//        {
//        }

//        //  The issue doesn't involve inheritance, part two:
//        //  Now we pass, as the 1-param ctor hides the 0-param ctor.
//        public class Passing_Two : Failing_One, IShowIssueNNNN
//        {
//            public Passing_Two(int dummy)
//            { }
//        }

//        //  The issue depends on the 0-param ctor, whether implicit or explicit:
//        //  We fail when we have both a 1-param ctor and a 0-param ctor.
//        public class Failing_Three : Failing_One, IShowIssueNNNN
//        {
//            public Failing_Three(int dummy)
//            { }

//            public Failing_Three()
//                : base()
//            { }
//        }

//        //  The issue depends on complex setter logic:
//        //  This version passes, despite its 0-param ctor.
//        //  It delegates directly to its backing store.
//        //  I only learned that this was an element of the issue while simplifying
//        // for the bug report.  In the original code, BackingStore is a
//        // different type from SerialItems, which is why SerialItems exists at all.
//        public class Passing_Three : IShowIssueNNNN
//        {
//            //  It also passes if we delete this ctor and leave it implicit.
//            public Passing_Three()
//                : base()
//            { }

//            [JsonIgnore]
//            public Dictionary<string, string> BackingStore { get; set; } = new Dictionary<string, string>();

//            public Dictionary<string, string> SerialItems
//            {
//                get { return BackingStore; }
//                set { BackingStore = value; }
//            }

//            public void AddItem(string item, string position)
//            {
//                BackingStore[position] = item;
//            }
//        }

//        //  The issue depends on a complex proxy type:
//        //  This version has a 0-param constructor and a complex setter
//        public class Passing_Four
//        {
//            // has 0-param constructor
//            public Passing_Four()
//            { }

//            [JsonIgnore]
//            public string BackingStore { get; set; } = string.Empty;

//            public string SerialProxy
//            {
//                get
//                {
//                    var intermediate = BackingStore.Clone().ToString();
//                    return intermediate.Trim();
//                }

//                // has a complex setter
//                set
//                {
//                    var intermediate = value.Clone().ToString();
//                    BackingStore = intermediate.Trim();
//                }
//            }
//        }

//        [Test]
//        public void Proxy_of_simple_type_deserializes_correctly()
//        {
//            var passing = new Passing_Four();

//            passing.BackingStore = " Wargh! ";

//            var json = JsonConvert.SerializeObject(passing);

//            var newPassing = JsonConvert.DeserializeObject<Passing_Four>(json);

//            Assert.AreEqual("Wargh!", newPassing.BackingStore);
//        }

//        //  The issue also occurs with List<string>:
//        public class Failing_Four
//        {
//            // has 0-param constructor
//            public Failing_Four()
//            { }

//            [JsonIgnore]
//            public List<string> BackingStore { get; set; } = new List<string>();

//            public List<string> SerialProxy
//            {
//                get
//                {
//                    var list = new List<string>();
//                    foreach (var str in BackingStore)
//                    {
//                        list.Add(str);
//                    }
//                    return list;
//                }

//                // has a complex setter
//                set
//                {
//                    BackingStore = new List<string>();
//                    foreach (var str in value)
//                    {
//                        BackingStore.Add(str);
//                    }
//                }
//            }
//        }

//        [Test]
//        public void Proxy_of_list_of_string_type_deserializes_wrong()
//        {
//            var passing = new Failing_Four();

//            passing.BackingStore.Add(" Wargh! ");
//            passing.BackingStore.Add("Bloit!");

//            var json = JsonConvert.SerializeObject(passing);

//            var newPassing = JsonConvert.DeserializeObject<Failing_Four>(json);

//            Assert.AreEqual(2, newPassing.BackingStore.Count);
//        }

//        //  The issue does not occur with string[]:
//        public class Passing_Five
//        {
//            // has 0-param constructor
//            public Passing_Five()
//            { }

//            [JsonIgnore]
//            public string[] BackingStore { get; set; } = new string[2];

//            public string[] SerialProxy
//            {
//                get
//                {
//                    var list = new string [BackingStore.Length];
//                    for (int i = 0; i < BackingStore.Length; i++)
//                    {
//                        list[i] = BackingStore[i];
//                    }
//                    return list;
//                }

//                // has a complex setter
//                set
//                {
//                    BackingStore = new string[value.Length];
//                    for (int i = 0; i < value.Length; i++)
//                    {
//                        BackingStore[i] = value[i];
//                    }
//                }
//            }
//        }

//        [Test]
//        public void Proxy_of_array_of_string_type_deserializes_correctly()
//        {
//            var passing = new Passing_Five();

//            passing.BackingStore[0] = " Wargh! ";
//            passing.BackingStore[1] = "Bloit!";

//            var json = JsonConvert.SerializeObject(passing);

//            var newPassing = JsonConvert.DeserializeObject<Passing_Five>(json);

//            Assert.AreEqual(2, newPassing.BackingStore.Length);
//            Assert.AreEqual("Bloit!", newPassing.BackingStore[1]);
//        }
//    }
//}
