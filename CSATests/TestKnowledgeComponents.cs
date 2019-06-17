using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.Tests
{
    public class TestKnowledgeComponents
    {
        private readonly ITestOutputHelper output;

        /*
         * Tests for KC_FloatVector. 
         */
        #region KC_FloatVector tests
        /*
         * Data and test method for testing KC_FloatVector constructors. 
         */
        #region KC_FloatVector constructor tests
        public static IEnumerable<object[]> Data_KC_FloatVector_Constructors()
        {
            KC_FloatVector fv1 = new KC_FloatVector();

            float[] floats1 = { 0.1F, 0.2F, 0.3F, 0.4F };
            KC_FloatVector fv2 = new KC_FloatVector(floats1);

            float[] floats2 = { 1.1F, 2.2F, 3.3F, 4.4F };
            KC_FloatVector fv3 = new KC_FloatVector(floats2, false);
            KC_FloatVector fv4 = new KC_FloatVector(floats2, true);

            KC_FloatVector fv5 = new KC_FloatVector(fv4);

            /*
             * Structure of object[]
             * KC_FloatVector - the KC_FloatVector to test
             * float[] - value of FloatVector
             * bool - value of ReadOnly
             */
            return new List<object[]>
            {
                // KC_FloatVector()
                new object[] { fv1, null, false },

                // KC_FloatVector(floats1)
                new object[] { fv2, floats1, false },

                // KC_FloatVector(floats2, false)
                new object[] { fv3, floats2, false },

                // KC_FloatVector(floats2, true)
                new object[] { fv4, floats2, true },

                // KC_FloatVector(fv4)
                new object[] { fv5, floats2, true },
            };
        }

        [Theory]
        [MemberData(nameof(Data_KC_FloatVector_Constructors))]
        public void Test_KC_FloatVector_Constructors(KC_FloatVector fv, float[] floatArray, bool readOnly)
        {
            Assert.False(fv.IsNormalized);
            Assert.Equal(floatArray, fv.FloatArray);
            Assert.Null(fv.NormalizedVector);
            Assert.Equal(readOnly, fv.ReadOnly);
        }
        #endregion

        /*
         * Method for testing KC_FloatVector.FloatVector assignment
         */
        #region KC_FloatVector assignment
        [Fact]
        public void Test_KC_FloatVector_Assignment()
        {
            float[] floats1 = { 0.1F, 0.2F, 0.3F };
            KC_FloatVector fv = new KC_FloatVector(floats1, false);
            float[] floats2 = { 1.0F, 2.0F, 3.0F, 4.0F };

            fv.FloatVector = floats2;
            Assert.Equal(floats2, fv.FloatVector);

            fv.FloatVector[0] = 0.0F;
            Assert.Equal(0.0F, fv.FloatVector[0]);

            float[] myFloats = (float[])fv.FloatVector;
            myFloats[1] = 0.0F;
            Assert.Equal(0.0F, fv.FloatVector[1]);

            fv.ReadOnly = true;

            try
            {
                fv.FloatVector = floats1;
                Assert.True(false, "Assignment to FloatVector property of a read only FloatVector succeeded");
            }
            catch (InvalidOperationException) { }

            try
            {
                fv.ReadOnly = false;
                Assert.True(false, "Assignment to ReadOnly property of a read only FloatVector suycceeded.");
            }
            catch (InvalidOperationException) { }

            try
            {
                fv.FloatVector[0] = 0.0F;
                Assert.True(false, "Assignment to FloatVector index of a read only FloatVector succeeded.");
            }
            catch (NotSupportedException) { }

            try
            {
                float[] myfloats = (float[])fv.FloatVector;
                myfloats[0] = 0.0F;
                Assert.True(false, "Assignment to FloatVector index of a read only FloatVector succeeded.");
            }
            catch (InvalidCastException) { }
        }
        #endregion

        /*
         * Test that changing clone doesn't change original
         */
        #region KC_FloatVector clone assignment test
        [Fact]
        public void Test_KC_FloatVector_CloneAssignment()
        {
            float[] floats = { 10F, 11F, 12F };
            var fv1 = new KC_FloatVector(floats);
            var fv2 = new KC_FloatVector(fv1);
            fv2.FloatVector[0] = 1F;
            Assert.NotEqual(fv1.FloatVector[0], fv2.FloatVector[0]);
        }
        #endregion

        /*
         * Utility method used to compare if two doubles are equal within a tolerance.
         */
        private bool EqualWithinTolerance(double d1, double d2)
        {
            double epsilon = d1 * 0.000001;
            return Math.Abs(d1 - d2) < epsilon;
        }

        /*
         * Method for testing KC_FloatVector.Length (both signatures)
         */
        #region KC_FloatVector length tests
        [Fact]
        public void Test_KC_FloatVector_Length()
        {
            float[] floats = { 1.0F, 2.0F, 3.0F, 4.0F, 5.0F };
            double len = KC_FloatVector.Length(floats);

            Assert.True(EqualWithinTolerance(len, 7.416198));

            KC_FloatVector fv = new KC_FloatVector(floats);
            Assert.True(EqualWithinTolerance(fv.Length(), 7.416198));
        }
        #endregion

        /*
         * Method for testing KC_FloatVector.Normalize()
         */
        #region KC_FloatVector.Normalize tests
        [Fact]
        public void Test_KC_FloatVector_Normalize()
        {
            float[] floats = { 0.1F, 0.2F, 0.3F, 0.4F, 0.5F };
            KC_FloatVector fv = new KC_FloatVector(floats);
            Assert.False(fv.IsNormalized);
            Assert.Null(fv.NormalizedVector);

            fv.Normalize();
            Assert.Equal(floats, fv.FloatVector);
            float[] normfloats = { 0.13483997249264842F, 0.26967994498529685F, 0.40451991747794525F, 0.5393598899705937F, 0.674199862463242F };
            for(int i = 0; i < floats.Length; i++)
            {
                 Assert.True(EqualWithinTolerance(normfloats[i], fv.NormalizedVector[i]));
            }

            float[] normalized = new float [fv.NormalizedVector.Count];
            fv.NormalizedVector.CopyTo(normalized, 0);
            double normlen = KC_FloatVector.Length(normalized);
            Assert.True(EqualWithinTolerance(1.0, normlen));
            Assert.True(fv.IsNormalized);
            fv.FloatVector = new float[] { 9.0F, 8.0F, 7.0F};
            Assert.False(fv.IsNormalized);
            Assert.Null(fv.NormalizedVector);
        }
        #endregion

        /*
         * Method for testing KC_FloatVector.Dot (both signatures).
         */
        #region KC_FloatVector.Dot test
        [Fact]
        public void Test_KC_FloatVector_Dot()
        {
            // Two random five dimensional arrays (generated in python)
            float[] vec1 = { 31.5202609F, 40.01996581F, 49.89208719F, 73.88465236F, 61.65325936F };
            float[] vec2 = { 60.74887955F, 69.51020825F, 21.68914134F, 24.71560093F, 86.70878694F };
            double dotProd = 12950.716133892063;

            var fv1 = new KC_FloatVector(vec1);
            var fv2 = new KC_FloatVector(vec2);

            Assert.True(EqualWithinTolerance(dotProd, fv1.Dot(vec2)));
            Assert.True(EqualWithinTolerance(dotProd, fv2.Dot(vec1)));
            Assert.True(EqualWithinTolerance(dotProd, fv1.Dot(fv2)));
            Assert.True(EqualWithinTolerance(dotProd, fv2.Dot(fv1)));

            float[] vec3 = null;
            float[] vec4 = { 0.1F, 0.2F };
            var fv3 = new KC_FloatVector();
            KC_FloatVector fv4 = null;

            // Attempt to take dot product with float vector that has not been initialized.
            try
            {
                fv1.Dot(vec3);
                Assert.True(false, "Dot product with null float[] succeeded.");
            }
            catch (ArgumentNullException) { }

            // Attempt to take dot product with uninitialized KC_FloatVector
            try
            {
                fv3.Dot(vec1);
                Assert.True(false, "Calling dot product on initialized KC_FloatVector succeeded.");
            }
            catch (InvalidOperationException) { }

            // Attempt to take dot product with vectors that are not the same length.
            try
            {
                fv1.Dot(vec4);
                Assert.True(false, "Dot product with two dissimilar length vectors succeeded.");
            }
            catch (ArgumentException) { }

            // Attempt to take dot product with null KC_FloatVector
            try
            {
                fv1.Dot(fv4);
                Assert.True(false, "Dot product with null KC_FloatVector succeeded.");
            }
            catch (ArgumentNullException) { }

            // Attempt to take dot product with KC_Float fector that has not been initialized
            try
            {
                fv1.Dot(fv3);
                Assert.True(false, "Dot product with uninitilized KC_FloatVector succeeded.");
            }
            catch (InvalidOperationException) { }

        }
        #endregion

        /*
         * Method for testing KC_FloatVector.CosignSim (both signatures)
         */
        #region KC_FloatVector.CosignSim test
        [Fact]
        public void Test_KC_FloatVector_CosineSim()
        {
            float[] vec1 = { 31.5202609F, 40.01996581F, 49.89208719F, 73.88465236F, 61.65325936F };
            float[] vec2 = { 60.74887955F, 69.51020825F, 21.68914134F, 24.71560093F, 86.70878694F };
            double cosignsim = 0.8263762988702843;

            var fv1 = new KC_FloatVector(vec1);
            var fv2 = new KC_FloatVector(vec2);

            // Test combinations without normalized vectors
            Assert.True(EqualWithinTolerance(cosignsim, fv1.CosineSim(vec2)));
            Assert.True(EqualWithinTolerance(cosignsim, fv2.CosineSim(vec1)));
            Assert.True(EqualWithinTolerance(cosignsim, fv1.CosineSim(fv2)));
            Assert.True(EqualWithinTolerance(cosignsim, fv2.CosineSim(fv1)));

            // Normalize first vector and test combos
            fv1.Normalize();
            Assert.True(EqualWithinTolerance(cosignsim, fv1.CosineSim(vec2)));
            Assert.True(EqualWithinTolerance(cosignsim, fv2.CosineSim(vec1)));
            Assert.True(EqualWithinTolerance(cosignsim, fv1.CosineSim(fv2)));
            Assert.True(EqualWithinTolerance(cosignsim, fv2.CosineSim(fv1)));

            // Normalize second vector and test combos
            fv2.Normalize();
            Assert.True(EqualWithinTolerance(cosignsim, fv1.CosineSim(vec2)));
            Assert.True(EqualWithinTolerance(cosignsim, fv2.CosineSim(vec1)));
            Assert.True(EqualWithinTolerance(cosignsim, fv1.CosineSim(fv2)));
            Assert.True(EqualWithinTolerance(cosignsim, fv2.CosineSim(fv1)));
        }
        #endregion

        #endregion

        public TestKnowledgeComponents(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}
