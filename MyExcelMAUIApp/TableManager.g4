grammar TableManager;

compileUnit : expression EOF;
expression :

    operatorToken=(ADD | SUBTRACT | INC | DEC) expression                       #UnaryExpr

    | expression operatorToken=(MULTIPLY | DIVIDE | DIV | MOD) expression       #MultiplicativeExpr
    | expression operatorToken=(ADD | SUBTRACT) expression                      #AdditiveExpr
    | LPAREN expression RPAREN                                                  #ParenthesizedExpr
    | NUMBER                                                                    #NumberExpr
    | IDENTIFIER                                                                #IdentifierExpr
    ;

NUMBER : INT;
IDENTIFIER : [a-zA-Z]+[0-9]+;
INT : [0-9]+;
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';

INC : [iI][nN][cC];
DEC : [dD][eE][cC];
DIV : [dD][iI][vV];
MOD : [mM][oO][dD];
LPAREN : '(';
RPAREN : ')';
WS : [ \t\r\n] -> channel(HIDDEN);
