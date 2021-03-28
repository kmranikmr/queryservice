grammar Hql;

@header {
}

query
: complexExpr EOF
| searchcomplexExpr EOF
;

complexExpr
: '(' complexExpr ')' 			#bracesComplexExpression
|complexExpr and complexExpr 	#andComplexExpression
|complexExpr or complexExpr 	#orComplexExpression
|not complexExpr				#notComplexExpression
|termExpr						#termExpression
|existsExpr						#termExpression
|rangeExpr						#rangeExpression
|complexExpr complexExpr  		#multipleComplexExpression
;

searchcomplexExpr
: '[' searchcomplexExpr ']' 			#bracesSearchComplexExpression
|searchcomplexExpr and searchcomplexExpr 	#andSearchComplexExpression
|searchcomplexExpr or searchcomplexExpr 	#orSearchComplexExpression
|not searchcomplexExpr				#notSearchComplexExpression
|termExpr						#termSearchExpression
|existsExpr						#termSearchExpression
|rangeExpr						#rangeSearchExpression
|searchcomplexExpr searchcomplexExpr  		#multipleSearchComplexExpression
;


termExpr
:field eq value
|field contains value
|value
;

existsExpr: field EXISTS;

rangeExpr
:field range value
;

and: AND;
or: OR;
not: NOT;

eq: EQ;
range: RANGE;
contains: CONTAINS;

AND: A N D;
OR:  O R;
NOT: N O T | '!';

dateValue : DATETIME;
floatValue: FLOAT;
intValue: INT;
date: DATE;
time: TIME;

term : WORD | IDENTIFIER;
phrase: PHRASE (TILDE phraseOptions)?;
field: IDENTIFIER;
phraseOptions: INT;

numberValue
: intValue
| floatValue
;

stringValue
: term
| phrase
;

value
: numberValue
| dateValue
| stringValue
;

DATETIME: DQUOTE DATE (WS TIME)? DQUOTE;
TIME: DIGIT? DIGIT ':' DIGIT? DIGIT (':' DIGIT? DIGIT)?;
DATE: (DIGIT DIGIT)? DIGIT DIGIT '-' DIGIT? DIGIT '-' DIGIT? DIGIT;
PHRASE: STRINGLITERAL;
INT: MINUS? POSITIVE_INT+;
POSITIVE_INT: DIGIT+;
FLOAT: INT '.' [0-9]+?;
IDENTIFIER: LETTER (LETTER | DIGIT | '.' | '-' | '_')*;
WORD:WORDCHARACTER+;
TILDE : '~';
EXISTS: '?';

fragment 
WORDCHARACTER
: ~('"' | '\\' | ' ' | '\t' | '\r' | '\n' | '=' | '>' | '<' | '!' | '('| ')' | '~' | '?');

STRINGLITERAL
: '"' STRINGCHARACTERS? '"'
;

fragment
STRINGCHARACTERS
: STRINGCHARACTER+
;

fragment
STRINGCHARACTER
: ~["\\]
| ESCAPESEQUENCE
;
fragment
ESCAPESEQUENCE
: '\\' [btnfr"'\\]
| UNICODESCAPE
;
fragment
UNICODESCAPE
: '\\' 'u' HEXDIGIT HEXDIGIT HEXDIGIT HEXDIGIT
;
fragment
HEXDIGIT
: [0-9a-fA-F]
;
EQ
:':'
|'='
;
RANGE
:'>'
|'<'
|'>='
|'<='; 
WS: [ \t\r\n]+ -> skip;
fragment DQUOTE: '"';
fragment DIGIT : [0-9];
fragment MINUS : '-';
fragment LETTER: [a-zA-Z];
fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];