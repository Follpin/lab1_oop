grammar LabCalculator;

/*
 * Parser Rules
 */
compileUnit : expression EOF ;

expression :
    LPAREN expression RPAREN                                              #ParenthesizedExpr
    | func=(MAX | MIN) LPAREN expression COMMA expression RPAREN          #FunctionExpr
    | operatorToken=(ADD | SUBTRACT) expression                           #UnaryExpr
    | expression operatorToken=(MULTIPLY | DIVIDE | MOD | DIV) expression #MultiplicativeExpr
    | expression operatorToken=(ADD | SUBTRACT) expression                #AdditiveExpr
    | NUMBER                                                              #NumberExpr
    | IDENTIFIER                                                          #IdentifierExpr
    ;

/*
 * Lexer Rules
 */
NUMBER : INT ('.' INT)? ;
IDENTIFIER : [a-zA-Z]+ [1-9] [0-9]* ;
INT : [0-9]+ ;

MULTIPLY : '*' ;
DIVIDE : '/' ;
MOD : [mM][oO][dD] ;
DIV : [dD][iI][vV] ;
ADD : '+' ;
SUBTRACT : '-' ;
MAX : [mM][aA][xX] ;
MIN : [mM][iI][nN] ;
COMMA : ',' ;
LPAREN : '(' ;
RPAREN : ')' ;

WS : [ \t\r\n]+ -> skip ;
