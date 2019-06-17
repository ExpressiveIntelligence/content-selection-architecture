using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    public class KC_FloatVector : KC_FloatArray
    {
        protected float[] m_normalizedVector;

        public ReadOnlyCollection<float> NormalizedVector => m_normalizedVector != null ? Array.AsReadOnly(m_normalizedVector) : null;

        public bool IsNormalized => m_normalizedVector != null;

        public IList<float> FloatVector
        {
            get => FloatArray;

            set
            {
                FloatArray = value;
                m_normalizedVector = null;
            }
        }

        public override string ToString()
        {
            return ToString("FloatVector");
        }

        public double Dot(float[] vector)
        {
            if (FloatVector == null)
            {
                throw new InvalidOperationException($"Attempt to take dot product of KC_FloatVector that has not been initialized: {this}");
            }
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector), "Attempt to take dot product of a KC_FloatVector with a null vector");
            }
            if (FloatVector.Count != vector.Length)
            {
                throw new ArgumentException("Attempt to take dot product of vectors that are not the same length", nameof(vector));
            }

            double dotProduct = 0.0;
            for (int i = 0; i < vector.Length; i++)
            {
                dotProduct += vector[i] * FloatVector[i];
            }
            return dotProduct;
        }

        public double Dot(KC_FloatVector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector), "Attempt to take dot product of a KC_FloatVector with a null KC_FloatVector");
            }
            if (vector.FloatVector == null)
            {
                throw new InvalidOperationException("Attempt to take dot product of KC_FloatVector with a KC_FloatVector that has not been initialized.");
            }
            return Dot((float[])vector.FloatVector);
        }

        public static double Dot(IList<float> vec1, IList<float> vec2)
        {
            if (vec1 == null)
            {
                throw new ArgumentNullException(nameof(vec1), "Attempt to take dot product with a null vector.");
            }

            if (vec2 == null)
            {
                throw new ArgumentNullException(nameof(vec2), "Attempt to take dot product with a null vector.");
            }

            if (vec1.Count != vec2.Count)
            {
                throw new ArgumentException("Attempt to take dot product of vectors that are not the same length.");
            }

            double dotProduct = 0.0;
            for (int i = 0; i < vec1.Count; i++)
            {
                dotProduct += vec1[i] * vec2[i];
            }
            return dotProduct;
        }

        public double CosineSim(float[] vector)
        {
            if (FloatVector == null)
            {
                throw new InvalidOperationException($"Attempt to compute the cosine similarity with a KC_FloatVector that has not been initialized: {this}");
            }
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector), "Attempt to compute the cosine similarity of a KC_FloatVector with a null vector");
            }
            if (FloatVector.Count != vector.Length)
            {
                throw new ArgumentException("Attempt to compute the cosine similarity of vectors that are not the same length", nameof(vector));
            }

            return IsNormalized ? Dot(m_normalizedVector, vector) / Length(vector) : Dot(vector) / (Length(vector) * Length());
        }

        public double CosineSim(KC_FloatVector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector), "Attempt to compute the cosine similarity KC_FloatVector with a null KC_FloatVector");
            }
            if (vector.FloatVector == null)
            {
                throw new InvalidOperationException("Attempt to compute the cosine similarity of KC_FloatVector with a KC_FloatVector that has not been initialized.");
            }

            IList<float> vec1 = IsNormalized ? m_normalizedVector : FloatVector;
            IList<float> vec2 = vector.IsNormalized ? vector.m_normalizedVector : vector.FloatVector;
            double len1 = IsNormalized ? 1.0 : Length();
            double len2 = vector.IsNormalized ? 1.0 : vector.Length();

            double dotProd = Dot(vec1, vec2);
            double lenProd = len1 * len2;
            return dotProd / lenProd;
        }

        public void Normalize()
        {
            if (!IsNormalized)
            {
                float len = (float)Length();
                m_normalizedVector = new float[FloatVector.Count];
                for (int i = 0; i < FloatVector.Count; i++)
                {
                    m_normalizedVector[i] = FloatVector[i] / len;
                }
             }
        }

        public double Length()
        {
            return Length((float[])FloatVector);
        }

        public static double Length(float[] vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector), "Attempt to compute the length of a null vector");
            }
            double product = 0.0;
            foreach (float f in vector)
            {
                product += f * f;
            }
            return Math.Sqrt(product);
        }

        //public static bool VectorEquals(float[] vec1, float[] vec2)
        //{
        //    if (vec1 == null || vec2 == null)
        //    {
        //        return vec1 == null && vec2 == null;
        //    }
        //    else
        //    {
        //        if (vec1.Length != vec2.Length)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            double epsilon = 
        //        }
        //    }


        //}

        public override object Clone() => new KC_FloatVector(this);

        public KC_FloatVector()
        {
        }

        public KC_FloatVector(float[] floats) : base(floats)
        {
        }

        public KC_FloatVector(float[] floats, bool readOnly) : base (floats, readOnly)
        {
        }

        public KC_FloatVector(KC_FloatArray toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_FloatVector_Extensions
    {
        public static IList<float> GetFloatVector(this Unit unit)
        {
            if (unit.HasComponent<KC_FloatVector>())
            {
                return unit.GetComponent<KC_FloatVector>().FloatVector;
            }
            throw new InvalidOperationException("GetFloatVector called on unit that does not have a KC_FloatVector component.");
        }

        public static void SetFloatVector(this Unit unit, float[] vector)
        {
            if (unit.HasComponent<KC_FloatVector>())
            {
                unit.GetComponent<KC_FloatVector>().FloatVector = vector;
            }
            throw new InvalidOperationException("SetFloatVector called on unit that does not have a KC_FloatVector component.");
        }

        public static bool GetIsFloatVectorNormalized(this Unit unit)
        {
            if (unit.HasComponent<KC_FloatVector>())
            {
                return unit.GetComponent<KC_FloatVector>().IsNormalized;
            }
            throw new InvalidOperationException($"nameof(GetIsFloatVectorNormalized) called on unit that does not have a KC_FloatVector component.");
        }

        public static void NormalizeFloatVector(this Unit unit)
        {
            if (unit.HasComponent<KC_FloatVector>())
            {
                unit.GetComponent<KC_FloatVector>().Normalize();
            }
            throw new InvalidOperationException("NormalizeFloatVector called on unit that does not have a KC_FloatVector component.");
        }

        public static ReadOnlyCollection<float> GetNormalizedFloatVector(this Unit unit)
        {
            if (unit.HasComponent<KC_FloatVector>())
            {
                return unit.GetComponent<KC_FloatVector>().NormalizedVector;
            }
            throw new InvalidOperationException("GetNormalizedFloatVector called on unit that does not have a KC_FloatVector component.");
        }
    }
}
