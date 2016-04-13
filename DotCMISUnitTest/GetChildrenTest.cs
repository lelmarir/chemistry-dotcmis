using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotCMIS;
using DotCMIS.Client.Impl;
using DotCMIS.Client;

namespace DotCMISUnitTest
{
    [TestFixture]
    class GetChildrenTest : TestFramework
    {
        private static int numOfDocuments = 250;

        [Test]
        public void TestPaging()
        {

            IOperationContext oc = Session.CreateOperationContext();
            oc.MaxItemsPerPage = 100;

            IFolder folder = createData(Session);

            int counter = 0;
            foreach (ICmisObject child in folder.GetChildren(oc))
            {
                Console.WriteLine("!" + counter + " " + child.Name);
                counter++;
            }

            Assert.AreEqual(numOfDocuments, counter);

            counter = 0;
            foreach (ICmisObject child in folder.GetChildren(oc).GetPage(150))
            {
                Console.WriteLine("#" + counter + " " + child.Name);
                counter++;
            }

            Assert.AreEqual(150, counter);

            counter = 0;
            foreach (ICmisObject child in folder.GetChildren(oc).SkipTo(20).GetPage(180))
            {
                Console.WriteLine("*" + counter + " " + child.Name);
                counter++;
            }

            Assert.AreEqual(180, counter);

            folder.DeleteTree(true, null, true);
        }


        private IFolder createData(ISession session)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[PropertyIds.Name] = "childrenTestFolder";
            properties[PropertyIds.ObjectTypeId] = "cmis:folder";

            IFolder folder = TestFolder.CreateFolder(properties);

            for (int i = 0; i < numOfDocuments; i++)
            {
                Dictionary<string, object> docProps = new Dictionary<string, object>();
                docProps[PropertyIds.Name] = "doc" + i.ToString();
                docProps[PropertyIds.ObjectTypeId] = "cmis:document";

                folder.CreateDocument(docProps, null, null);

            }

            return folder;
        }
    }
}
