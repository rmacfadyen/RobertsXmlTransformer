using System;
using System.Collections.Generic;
using System.Text;

namespace RobertsXmlTransformer
{
    class Transform
    {
        public TransformTypes TransformType { get; private set; }
        public string TransformContent { get; private set; }
        public IList<string> TransformAttributes { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="TransformAttributeValue"></param>
        /// <param name="TransformType"></param>
        /// <param name="TransformContent"></param>
        public static Transform Parse(string TransformAttributeValue)
        {
            var Transform = new Transform();

            TransformAttributeValue = TransformAttributeValue.Trim();
            string RawTransformType;

            if (TransformAttributeValue.IndexOf("(", StringComparison.OrdinalIgnoreCase) == -1)
            {
                RawTransformType = TransformAttributeValue;
                Transform.TransformContent = null;
                Transform.TransformAttributes = null;
            }
            else
            {
                if (!TransformAttributeValue.EndsWith(")"))
                {
                    throw new ArgumentException(
                        $"Invalid Transform value '{TransformAttributeValue}', does not end with a close parenthesis");
                }

                var ParenthesisLocation = TransformAttributeValue.IndexOf("(", StringComparison.OrdinalIgnoreCase);
                RawTransformType = TransformAttributeValue.Substring(0, ParenthesisLocation);
                Transform.TransformContent = TransformAttributeValue.Substring(ParenthesisLocation + 1, TransformAttributeValue.Length - ParenthesisLocation - 2);
                Transform.TransformAttributes = new List<string>(Transform.TransformContent.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }

            //
            // Make sure the transform type is valid and the transformcontent/attributes are present if required
            //
            switch (RawTransformType.ToUpperInvariant())
            {
                case "REPLACE":
                    Transform.TransformType = TransformTypes.Replace;
                    break;

                case "INSERT":
                    Transform.TransformType = TransformTypes.Insert;
                    break;

                case "REMOVE":
                    Transform.TransformType = TransformTypes.Remove;
                    break;

                case "REMOVEALL":
                    Transform.TransformType = TransformTypes.RemoveAll;
                    break;

                case "SETATTRIBUTES":
                    Transform.TransformType = TransformTypes.SetAttributes;
                    break;

                case "REMOVEPREVIOUSCOMMENT":
                    Transform.TransformType = TransformTypes.RemovePreviousComment;
                    break;

                case "REMOVEFOLLOWINGCOMMENT":
                    Transform.TransformType = TransformTypes.RemoveFollowingComment;
                    break;

                case "COMMENT":
                    if (string.IsNullOrEmpty(Transform.TransformContent))
                    {
                        throw new ArgumentException("Transform Comment cannot have an empty argument");
                    }
                    Transform.TransformType = TransformTypes.Comment;
                    break;

                case "INSERTBEFORE":
                    if (string.IsNullOrEmpty(Transform.TransformContent))
                    {
                        throw new ArgumentException("Transform InsertAfter cannot have an empty XPath argument");
                    }
                    Transform.TransformType = TransformTypes.InsertBefore;
                    break;

                case "INSERTAFTER":
                    if (string.IsNullOrEmpty(Transform.TransformContent))
                    {
                        throw new ArgumentException("Transform InsertAfter cannot have an empty XPath argument");
                    }
                    Transform.TransformType = TransformTypes.InsertAfter;
                    break;

                case "REMOVEATTRIBUTES":
                    if (Transform.TransformAttributes == null || Transform.TransformAttributes.Count == 0)
                    {
                        throw new ArgumentException("Transform RemoveAttributes cannot have an empty list of attributes");
                    }
                    Transform.TransformType = TransformTypes.RemoveAttributes;
                    break;

                default:
                    throw new ArgumentException($"Invalid Transform type: {RawTransformType}");
            }

            return Transform;
        }
    }
}
