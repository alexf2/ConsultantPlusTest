<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl = "urn:schemas-microsoft-com:xslt"
                xmlns:my="http:/consultant.ru/nris/test"
                xmlns:VEO = "urn:ValidatorExtensionObj"
                xmlns = "http://www.w3.org/1999/xhtml"
                xmlns:ifx = "urn:ifx:view-script"
                exclude-result-prefixes="xsl msxsl my VEO ifx" xml:space ="default">

  <xsl:output method = "xml" indent = "yes" omit-xml-declaration = "yes" encoding = "UTF-8" />

  <xsl:strip-space elements = "" />
  <xsl:preserve-space elements = "style" />

  <msxsl:script language = "JScript" implements-prefix = "ifx" >
    <![CDATA[
      function getGroupNumber (grpName)
      {
        if (typeof(grpName) == "undefined" || !grpName)
          return -1;
          
        var n = grpName.match(/\d+$/i);        

        if (n.length >= 1)
          return parseInt(n[0]);
         else
          return -1;
      }
    ]]>
  </msxsl:script>

  <xsl:template match = "/" >
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match = "my:Data">    
    <xsl:element name="html">
      <xsl:call-template name = "Head">
        <xsl:with-param name="title" select = "concat(@my:attrib1, ' - ', my:attrib2)" />
      </xsl:call-template>

      <xsl:element name="body" >
        <xsl:apply-templates select= "my:Entity | node()[starts-with(name(.), 'my:Group')]" />
      </xsl:element>

    </xsl:element>
  </xsl:template>


  <xsl:template match="node()[starts-with(name(.), 'my:Group')]">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match = "my:Entity">    
    <xsl:variable name="is-top-level-entry">
      <xsl:value-of select="count(ancestor::node()[starts-with(name(.), 'my:Group')]) = 0"/>
    </xsl:variable>    

    <xsl:choose>
      <xsl:when test = "$is-top-level-entry = 'true'">
        <xsl:call-template name = "TopLevelEntry" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name = "EntryInGroup" />
      </xsl:otherwise>
    </xsl:choose>      
</xsl:template>

  <xsl:template name = "TopLevelEntry">
    <xsl:variable name="entry-level">
      <xsl:value-of select="count(ancestor::node()[name(.) = 'my:Entity'])"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test = "$entry-level = 0">
        <center>
          <xsl:apply-templates select= "my:Entity | my:RichText" />
        </center>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select= "my:Entity | my:RichText" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name = "EntryInGroup">
    <xsl:variable name="entry-level">
      <xsl:value-of select="count(ancestor::node()[name(.) = 'my:Entity'])"/>
    </xsl:variable>    

    <div class="{concat('ent', $entry-level)}">
      <xsl:apply-templates select= "my:Entity | my:RichText" />
    </div>
  </xsl:template>

  <xsl:template match = "my:RichText">
    <xsl:variable name="entry-level">
      <xsl:value-of select="count(ancestor::node()[name(.) = 'my:Entity'])"/>
    </xsl:variable>

    <xsl:variable name="is_first-entry">
      <xsl:value-of select="count(./parent::my:Entity/preceding-sibling::my:Entity) = 0"/>
    </xsl:variable>
    
    <xsl:variable name="group-number">
      <xsl:value-of select="ifx:getGroupNumber( local-name(ancestor::node()[starts-with(name(.), 'my:Group')][1]) )"/>
    </xsl:variable>    

    <xsl:variable name="is-top-level-entry">
      <xsl:value-of select="count(ancestor::node()[starts-with(name(.), 'my:Group')]) = 0"/>
    </xsl:variable>

    <xsl:if test="$is-top-level-entry = 'true'">      
      <xsl:if test="$entry-level = 0">
        <h1>
          <xsl:value-of select="VEO:ValidatingFilter(.)" disable-output-escaping="yes" />
        </h1>
      </xsl:if>
      <xsl:if test="$entry-level = 1">
        <h2>
          <xsl:value-of select="VEO:ValidatingFilter(.)" disable-output-escaping="yes" />
        </h2>
      </xsl:if>
      <xsl:if test="$entry-level = 2">
        <h3>
          <xsl:value-of select="VEO:ValidatingFilter(.)" disable-output-escaping="yes" />
        </h3>
      </xsl:if>
    </xsl:if>

    <xsl:if test="$is-top-level-entry = 'false'">
      
      <xsl:if test="$group-number > 1">
        <br/>
      </xsl:if>
      
        <xsl:if test="$entry-level = 1 and $is_first-entry = 'true'">
          
          <b>
            <big>
              <xsl:value-of select="$group-number" />
            </big>
          </b>.&#160;&#160;
          
        </xsl:if>
      
        <xsl:value-of select="VEO:ValidatingFilter(.)" disable-output-escaping="yes" />
      
    </xsl:if>
    
  </xsl:template>

  <xsl:template name = "Head">
    <xsl:param name="title" />

    <xsl:element name="head" >
      <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
      <title><xsl:value-of select="$title" /></title>
      
      <xsl:text disable-output-escaping="yes"><![CDATA[<script type = "text/javascript" src = "http://code.jquery.com/jquery-1.10.2.min.js"></script>]]>&#xa;</xsl:text>

      <style>
        *.ent1 {padding-left: 1em;}
        *.ent2 {padding-left: 2em;}
        *.ent3 {padding-left: 3em;}
        *.ent4 {padding-left: 4em;}
        *.err {color: red;}
      </style>
      
    </xsl:element>
  </xsl:template>
  
  <!-- Default copy templates -->
  <!--xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template-->

  <xsl:template match="@*">
    <xsl:copy-of select = "." />
  </xsl:template>

  <xsl:template match = "processing-instruction()" >
    <xsl:copy />
  </xsl:template>
  
</xsl:stylesheet>
