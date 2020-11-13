using System;
using System.Collections.Generic;
using System.Text;

namespace RobertsXmlTransformer
{
    internal enum TransformTypes
    {
        Replace = 1,
        Insert,
        InsertBefore,
        InsertAfter,
        Remove,
        RemoveAll,
        RemoveAttributes,
        SetAttributes,
        Comment,
        RemovePreviousComment,
        RemoveFollowingComment
    }
}
