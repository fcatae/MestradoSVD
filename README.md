# Classificação automática de texto por meio de similaridade de palavras: um algoritmo mais eficiente

A análise da semântica latente é uma técnica de processamento de linguagem natural, que busca simplificar a 
tarefa de encontrar palavras e sentenças por similaridade. Através da representação de texto em um espaço multidimensional, 
selecionam-se os valores mais significativos para sua reconstrução em uma dimensão reduzida. Essa simplificação lhe confere 
a capacidade de generalizar modelos, movendo as palavras e os textos para uma representação semântica. Dessa forma, essa 
técnica identifica um conjunto de significados ou conceitos ocultos sem a necessidade do conhecimento prévio da gramática. 

O objetivo desse trabalho foi determinar a dimensionalidade ideal do espaço semântico em uma tarefa de classificação de texto. 
A solução proposta corresponde a um algoritmo semi-supervisionado que, a partir de exemplos conhecidos, aplica o método de 
classificação pelo vizinho mais próximo e determina uma curva estimada da taxa de acerto. Como esse processamento é demorado, 
os vetores são projetados em um espaço no qual o cálculo se torna incremental. Devido à isometria dos espaços, a similaridade 
entre documentos se mantém equivalente. Esta proposta permite determinar a dimensão ideal do espaço semântico com pouco esforço 
além do tempo requerido pela análise da semântica latente tradicional. Os resultados mostraram ganhos significativos em adotar 
o número correto de dimensões

**Link do site da USP:**
[](http://www.teses.usp.br/teses/disponiveis/3/3141/tde-06072014-225124/pt-br.php)

## Código Fonte

O programa é composto por dois módulos: Parser e SVD.

- Parser é um programa escrito em C# para fazer parser de arquivos e transformar os atributos.
- SVD calcula a decomposição matricial (single value decomposition). É um porte do programa do Iury em C++.


