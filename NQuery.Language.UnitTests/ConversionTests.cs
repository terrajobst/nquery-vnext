using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class ConversionTests
    {
        private static void HasConversion(Type souceType, Type targetType, bool isImplicit, bool viaMethod)
        {
            var conversion = Helpers.ClassifyConversion(souceType, targetType);
            var isExplicit = !isImplicit;
            var expectedMethodCount = viaMethod ? 1 : 0;

            Assert.IsTrue(conversion.Exists);
            Assert.AreEqual(isImplicit, conversion.IsImplicit);
            Assert.AreEqual(isExplicit, conversion.IsExplicit);
            Assert.IsFalse(conversion.IsIdentity);
            Assert.IsFalse(conversion.IsBoxing);
            Assert.IsFalse(conversion.IsUnboxing);
            Assert.IsFalse(conversion.IsReference);
            Assert.AreEqual(expectedMethodCount, conversion.ConversionMethods.Count);
        }

        private static void AssertIdentityConversion(Type type)
        {
            var conversion = Helpers.ClassifyConversion(type, type);

            Assert.IsTrue(conversion.Exists);
            Assert.IsTrue(conversion.IsIdentity);
            Assert.IsTrue(conversion.IsImplicit);
            Assert.IsFalse(conversion.IsExplicit);
            Assert.IsFalse(conversion.IsBoxing);
            Assert.IsFalse(conversion.IsUnboxing);
            Assert.IsFalse(conversion.IsReference);
            Assert.AreEqual(0, conversion.ConversionMethods.Count);
        }

        private static void AssertHasImplicitIntrinsicConversion(Type souceType, Type targetType)
        {
            HasConversion(souceType, targetType, true, false);
        }

        private static void AssertHasExplicitIntrinsicConversion(Type souceType, Type targetType)
        {
            HasConversion(souceType, targetType, false, false);
        }

        private static void AssertHasImplicitConversionViaMethod(Type souceType, Type targetType)
        {
            HasConversion(souceType, targetType, true, true);
        }

        private static void AssertHasExplicitConversionViaMethod(Type souceType, Type targetType)
        {
            HasConversion(souceType, targetType, false, true);
        }

        [TestMethod]
        public void Conversion_ClassifiesIdentityConversionsCorrectly()
        {
            // Known types
            AssertIdentityConversion(typeof(sbyte));
            AssertIdentityConversion(typeof(byte));
            AssertIdentityConversion(typeof(short));
            AssertIdentityConversion(typeof(ushort));
            AssertIdentityConversion(typeof(int));
            AssertIdentityConversion(typeof(uint));
            AssertIdentityConversion(typeof(long));
            AssertIdentityConversion(typeof(ulong));
            AssertIdentityConversion(typeof(char));
            AssertIdentityConversion(typeof(float));
            AssertIdentityConversion(typeof(double));
            AssertIdentityConversion(typeof(bool));
            AssertIdentityConversion(typeof(string));
            AssertIdentityConversion(typeof(object));

            // Other
            AssertIdentityConversion(typeof(decimal));
        }

        [TestMethod]
        public void Conversion_ClassifiesImplicitIntrinsicConversionsCorrectly()
        {
            AssertHasImplicitIntrinsicConversion(typeof(sbyte), typeof(short));
            AssertHasImplicitIntrinsicConversion(typeof(sbyte), typeof(int));
            AssertHasImplicitIntrinsicConversion(typeof(sbyte), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(sbyte), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(sbyte), typeof(double));

            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(short));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(ushort));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(int));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(uint));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(ulong));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(byte), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(short), typeof(int));
            AssertHasImplicitIntrinsicConversion(typeof(short), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(short), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(short), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(ushort), typeof(int));
            AssertHasImplicitIntrinsicConversion(typeof(ushort), typeof(uint));
            AssertHasImplicitIntrinsicConversion(typeof(ushort), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(ushort), typeof(ulong));
            AssertHasImplicitIntrinsicConversion(typeof(ushort), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(ushort), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(int), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(int), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(int), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(uint), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(uint), typeof(ulong));
            AssertHasImplicitIntrinsicConversion(typeof(uint), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(uint), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(long), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(long), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(ulong), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(ulong), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(ushort));
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(int));
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(uint));
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(long));
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(ulong));
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(float));
            AssertHasImplicitIntrinsicConversion(typeof(char), typeof(double));
            
            AssertHasImplicitIntrinsicConversion(typeof(float), typeof(double));
        }

        [TestMethod]
        public void Conversion_ClassifiesExplicitIntrinsicConversionsCorrectly()
        {
            AssertHasExplicitIntrinsicConversion(typeof(sbyte), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(sbyte), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(sbyte), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(sbyte), typeof(ulong));
            AssertHasExplicitIntrinsicConversion(typeof(sbyte), typeof(char));
        
            AssertHasExplicitIntrinsicConversion(typeof(byte), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(byte), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(short), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(short), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(short), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(short), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(short), typeof(ulong));
            AssertHasExplicitIntrinsicConversion(typeof(short), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(ushort), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(ushort), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(ushort), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(ushort), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(ulong));
            AssertHasExplicitIntrinsicConversion(typeof(int), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(uint), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(uint), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(uint), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(uint), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(uint), typeof(int));
            AssertHasExplicitIntrinsicConversion(typeof(uint), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(int));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(ulong));
            AssertHasExplicitIntrinsicConversion(typeof(long), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(int));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(long));
            AssertHasExplicitIntrinsicConversion(typeof(ulong), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(char), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(char), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(char), typeof(short));
            
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(int));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(long));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(ulong));
            AssertHasExplicitIntrinsicConversion(typeof(float), typeof(char));
            
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(sbyte));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(byte));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(short));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(ushort));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(int));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(uint));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(long));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(ulong));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(char));
            AssertHasExplicitIntrinsicConversion(typeof(double), typeof(float));
        }

        [TestMethod]
        public void Conversion_ClassifiesImplicitDecimalConversionsCorrectly()
        {
            AssertHasImplicitConversionViaMethod(typeof(sbyte), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(byte), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(short), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(ushort), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(int), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(uint), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(long), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(ulong), typeof(decimal));
            AssertHasImplicitConversionViaMethod(typeof(char), typeof(decimal));
        }

        [TestMethod]
        public void Conversion_ClassifiesExplicitDecimalConversionsCorrectly()
        {
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(sbyte));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(byte));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(short));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(ushort));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(int));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(uint));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(long));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(ulong));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(char));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(float));
            AssertHasExplicitConversionViaMethod(typeof(decimal), typeof(double));

            AssertHasExplicitConversionViaMethod(typeof(float), typeof(decimal));
            AssertHasExplicitConversionViaMethod(typeof(double), typeof(decimal));
        }

        [TestMethod]
        public void Conversion_ClassifiesBoxingCorrectly()
        {
            var conversion = Helpers.ClassifyConversion(typeof (int), typeof (object));
        
            Assert.IsTrue(conversion.Exists);
            Assert.IsTrue(conversion.IsImplicit);
            Assert.IsFalse(conversion.IsExplicit);
            Assert.IsFalse(conversion.IsIdentity);
            Assert.IsTrue(conversion.IsBoxing);
            Assert.IsFalse(conversion.IsUnboxing);
            Assert.IsFalse(conversion.IsReference);
            Assert.AreEqual(0, conversion.ConversionMethods.Count);
        }

        [TestMethod]
        public void Conversion_ClassifiesUnboxingCorrectly()
        {
            var conversion = Helpers.ClassifyConversion(typeof(object), typeof(int));

            Assert.IsTrue(conversion.Exists);
            Assert.IsFalse(conversion.IsImplicit);
            Assert.IsTrue(conversion.IsExplicit);
            Assert.IsFalse(conversion.IsIdentity);
            Assert.IsFalse(conversion.IsBoxing);
            Assert.IsTrue(conversion.IsUnboxing);
            Assert.IsFalse(conversion.IsReference);
            Assert.AreEqual(0, conversion.ConversionMethods.Count);
        }

        [TestMethod]
        public void Conversion_ClassifiesUpCastCorrectly()
        {
            var conversion = Helpers.ClassifyConversion(typeof(string), typeof(object));

            Assert.IsTrue(conversion.Exists);
            Assert.IsTrue(conversion.IsImplicit);
            Assert.IsFalse(conversion.IsExplicit);
            Assert.IsFalse(conversion.IsIdentity);
            Assert.IsFalse(conversion.IsBoxing);
            Assert.IsFalse(conversion.IsUnboxing);
            Assert.IsTrue(conversion.IsReference);
            Assert.AreEqual(0, conversion.ConversionMethods.Count);
        }

        [TestMethod]
        public void Conversion_ClassifiesDownCastCorrectly()
        {
            var conversion = Helpers.ClassifyConversion(typeof(object), typeof(string));

            Assert.IsTrue(conversion.Exists);
            Assert.IsFalse(conversion.IsImplicit);
            Assert.IsTrue(conversion.IsExplicit);
            Assert.IsFalse(conversion.IsIdentity);
            Assert.IsFalse(conversion.IsBoxing);
            Assert.IsFalse(conversion.IsUnboxing);
            Assert.IsTrue(conversion.IsReference);
            Assert.AreEqual(0, conversion.ConversionMethods.Count);
        }

    }
}