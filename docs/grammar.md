# NQuery Grammar

This document describes the grammar of the NQuery query language. It is
intended as a reference for writing queries and expressions.

The grammar is written in ANTLR4 syntax:

- `rule : ... ;` — a rule, terminated by a semicolon
- `|` — alternatives
- `x?` — optional (zero or one)
- `x*` — zero or more
- `x+` — one or more
- `( ... )` — grouping
- `'...'` — literal text (terminal)
- `//` and `/* ... */` — comments
- `EOF` — the end of the input

Names in `lower_case` (with underscores) are parser rules; names in `UPPERCASE`
are tokens (see [Lexical grammar](#lexical-grammar)). Keyword matching is
case-insensitive (ANTLR's `caseInsensitive` lexer option): `SELECT`, `select`,
and `Select` are equivalent.

## Input

An input is either a query or a single expression, followed by the end of the
input. Any text after the query or expression is an error.

```antlr
input
    : query EOF
    | expression EOF
    ;
```

## Queries

In `query_expression`, precedence is set by alternative order: `INTERSECT` binds
tighter than `UNION` and `EXCEPT`, and `ORDER BY` applies last, at the outermost
level. A parenthesized query may carry its own `ORDER BY`.

```antlr
query
    : ('WITH' common_table_expression_list)? query_expression
    ;

query_expression
    : select_query
    | query_expression 'INTERSECT' query_expression
    | query_expression ('UNION' 'ALL'? | 'EXCEPT') query_expression
    | query_expression 'ORDER' 'BY' order_by_column_list
    ;

select_query
    : '(' query ')'
    | select_clause from_clause? where_clause? group_by_clause? having_clause?
    ;
```

### Common table expressions

```antlr
common_table_expression_list
    : common_table_expression (',' common_table_expression)*
    ;

common_table_expression
    : 'RECURSIVE'? IDENTIFIER cte_column_name_list? 'AS' '(' query ')'
    ;

cte_column_name_list
    : '(' IDENTIFIER (',' IDENTIFIER)* ')'
    ;
```

### Select clause

```antlr
select_clause
    : 'SELECT' ('DISTINCT' | 'ALL')? top_clause? select_column_list
    ;

top_clause
    : 'TOP' NUMERIC_LITERAL ('WITH' 'TIES')?
    ;

select_column_list
    : select_column (',' select_column)*
    ;

select_column
    : '*'                          // wildcard
    | IDENTIFIER '.' '*'           // qualified wildcard
    | expression alias?            // expression column
    ;

alias
    : 'AS'? IDENTIFIER
    ;
```

The `TOP` value must be an integer.

### From clause and table references

Joins are left-associative.

```antlr
from_clause
    : 'FROM' table_reference_list
    ;

table_reference_list
    : table_reference (',' table_reference)*
    ;

table_reference
    : '(' query ')' 'AS'? IDENTIFIER                 // derived table
    | '(' table_reference ')'                        // parenthesized
    | IDENTIFIER alias?                              // named table
    | table_reference 'CROSS' 'JOIN' table_reference
    | table_reference 'CROSS' 'APPLY' table_reference
    | table_reference 'OUTER' 'APPLY' table_reference
    | table_reference 'INNER'? 'JOIN' table_reference 'ON' expression
    | table_reference ('LEFT' | 'RIGHT' | 'FULL') 'OUTER'? 'JOIN' table_reference 'ON' expression
    ;
```

`APPLY`, `LEFT`, and `RIGHT` are contextual keywords: they act as keywords only
in the positions shown above and may otherwise be used as identifiers.

### Other query clauses

```antlr
where_clause
    : 'WHERE' expression
    ;

group_by_clause
    : 'GROUP' 'BY' group_by_column_list
    ;

group_by_column_list
    : expression (',' expression)*
    ;

having_clause
    : 'HAVING' expression
    ;

order_by_column_list
    : order_by_column (',' order_by_column)*
    ;

order_by_column
    : expression ('ASC' | 'DESC')?
    ;
```

## Expressions

Operators have the following precedence, from lowest to highest binding; an
operator higher in the table binds more tightly. All binary operators are
left-associative.

| Precedence | Operators | Kind |
| ---------: | --------- | ---- |
| 1  | `OR` | binary |
| 2  | `AND` | binary |
| 3  | `LIKE`, `SOUNDS LIKE`, `SIMILAR TO`, `IN` | binary / pattern |
| 4  | `NOT` | unary prefix |
| 5  | `&`, `\|`, `^`, `<<`, `>>` | binary (bitwise / shift) |
| 6  | `=`, `<>`, `!=`, `<`, `<=`, `>`, `>=`, `!<`, `!>` | binary (comparison) |
| 7  | `+`, `-`; `BETWEEN ... AND ...` | binary (additive); ternary |
| 8  | `*`, `/`, `%` | binary (multiplicative) |
| 9  | `+`, `-`, `~` | unary prefix |
| 10 | `**` | binary (power) |

In the `expression` rule, precedence is set by alternative order: earlier
alternatives bind more tightly. `LIKE`, `IN`, `BETWEEN`, `SIMILAR TO`, and
`SOUNDS LIKE` accept an optional leading `NOT` (for example, `x NOT IN (1, 2)`).

```antlr
expression
    : literal                                                  // NULL, TRUE, FALSE, number, string, date
    | '@' IDENTIFIER                                           // variable
    | 'EXISTS' '(' query ')'
    | 'CAST' '(' expression 'AS' IDENTIFIER ')'
    | case_expression
    | 'COALESCE' '(' argument_list ')'                         // >= 2 arguments
    | 'NULLIF' '(' expression ',' expression ')'
    | 'COUNT' '(' '*' ')'                                      // count-all
    | IDENTIFIER '(' argument_list ')'                         // function invocation
    | IDENTIFIER                                               // column / name
    | '(' query ')'                                            // single-row subselect
    | '(' expression ')'                                       // parenthesized
    | expression '.' IDENTIFIER ('(' argument_list ')')?       // property access / method invocation
    | expression 'IS' 'NOT'? 'NULL'
    | expression '**' expression
    | ('+' | '-' | '~') expression
    | expression ('*' | '/' | '%') expression
    | expression ('+' | '-') expression
    | expression 'NOT'? 'BETWEEN' expression 'AND' expression
    | expression compare_op expression
    | expression compare_op ('ALL' | 'ANY' | 'SOME') '(' query ')'
    | expression ('&' | '|' | '^' | '<<' | '>>') expression
    | 'NOT' expression
    | expression 'NOT'? 'LIKE' expression
    | expression 'NOT'? 'SOUNDS' 'LIKE' expression
    | expression 'NOT'? 'SIMILAR' 'TO' expression
    | expression 'NOT'? 'IN' '(' argument_list ')'
    | expression 'NOT'? 'IN' '(' query ')'
    | expression 'AND' expression
    | expression 'OR' expression
    ;

compare_op
    : '=' | '<>' | '!=' | '<' | '<=' | '>' | '>=' | '!<' | '!>'
    ;

literal
    : 'NULL' | 'TRUE' | 'FALSE' | NUMERIC_LITERAL | STRING_LITERAL | DATE_LITERAL
    ;

case_expression
    : 'CASE' expression? case_label+ ('ELSE' expression)? 'END'
    ;

case_label
    : 'WHEN' expression 'THEN' expression
    ;

argument_list
    : (expression (',' expression)*)?
    ;
```

`COUNT(*)` is a special form. `COALESCE` requires at least two arguments and
`NULLIF` requires exactly two. A parenthesized construct beginning with
`SELECT` is a scalar (single-row) subselect; otherwise it is a parenthesized
expression. `IS [NOT] NULL` is a postfix test that binds tighter than every
binary operator.

## Lexical grammar

The token rules below describe the terminals used above.

### Identifiers

```antlr
IDENTIFIER
    : (LETTER | '_') (LETTER | DIGIT | '_' | '$')*   // regular
    | '"' (~["] | '""')* '"'                         // quoted
    | '[' (~[\]] | ']]')* ']'                        // bracketed
    ;
```

A regular identifier that spells a reserved keyword is treated as that keyword
(see [Keywords](#keywords)). Quoted (`"..."`) and bracketed (`[...]`)
identifiers are never keywords and may contain characters that are otherwise
not allowed in an identifier; the closing delimiter is escaped by doubling it.

### Literals

```antlr
NUMERIC_LITERAL
    : DIGIT* '.' DIGIT+ EXPONENT?
    | DIGIT+ EXPONENT
    | DIGIT+
    ;

fragment EXPONENT
    : ('e' | 'E') ('+' | '-')? DIGIT+
    ;

STRING_LITERAL
    : '\'' (~['] | '\'\'')* '\''
    ;

DATE_LITERAL
    : '#' ~[#\r\n]+ '#'
    ;

fragment LETTER : [a-zA-Z] ;
fragment DIGIT  : [0-9] ;
```

A numeric literal with a decimal point or exponent is a floating-point
number; otherwise it is an integer. A string literal escapes a single quote
by doubling it (`''`). A date literal is delimited by `#` and must be a
valid date.

### Operators and punctuation

```
~   &   |   ^   @   (   )   +   -   *   /   %   **
,   .   =   !=  <>  <   <=  >   >=  !<  !>  <<  >>
```

### Comments and whitespace

Comments and whitespace may appear between tokens and are ignored:

```antlr
LINE_COMMENT  : ('--' | '//') ~[\r\n]* -> skip ;
BLOCK_COMMENT : '/*' .*? '*/'           -> skip ;
WHITESPACE    : [ \t\r\n]+              -> skip ;
```

### Keywords

**Reserved keywords** cannot be used as identifiers unless quoted or bracketed:

```
ALL        AND        ANY        AS         ASC        BETWEEN
BY         CASE       CAST       COALESCE   CROSS      DESC
DISTINCT   ELSE       END        EXCEPT     EXISTS     FALSE
FROM       FULL       GROUP      HAVING     IN         INNER
INTERSECT  IS         JOIN       LIKE       NOT        NULL
NULLIF     ON         OR         ORDER      OUTER      RECURSIVE
SELECT     SIMILAR    SOME       SOUNDS     THEN       TIES
TO         TOP        TRUE       UNION      WHEN       WHERE
WITH
```

**Contextual keywords** act as keywords only in specific positions and may
otherwise be used as identifiers:

```
APPLY      LEFT       RIGHT
```
