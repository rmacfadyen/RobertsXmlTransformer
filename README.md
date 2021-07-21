# RobertsXmlTransformer

This library implements Microsoft's MSBuild XML transformation language. 
This was necesary because once upon a time Microsoft's implementation could
not be distributed.

The library is functionally equivelent to 
[XML-Document-Transform](https://docs.microsoft.com/en-us/previous-versions/aspnet/dd465326(v=vs.110))
with three additional commands.

#### Remove Previous / Following Comment

To remove a comment in an XML file the RemovePreviousComment or RemoveFollowingComment transform can be used:

    <?xml version="1.0"?>
    <a xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
      <b>
        <c key="abc" xdt:Locator="Match(key)" xdt:Transform="RemovePreviousComment" />
      </b>
    </a>

or

    <?xml version="1.0"?>
    <a xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
      <b>
        <c key="abc" xdt:Locator="Match(key)" xdt:Transform="RemoveFollowingComment" />
      </b>
    </a>

#### Add Comment

To add a comment in an XML file the Comment transform can be used:

    <?xml version="1.0"?>
    <a xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
      <b key="abc" xdt:Locator="Match(key)" xdt:Transform="Comment(abc"></b>
    </a>
