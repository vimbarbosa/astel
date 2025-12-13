-- =============================================
-- Script GERADOR: Gera comandos UPDATE com valores literais
-- Execute este script no ambiente ONDE EXISTE a tabela formapgto
-- Copie os resultados e cole no arquivo update_forma_pagamento_literal.sql
-- =============================================

USE ASTEL;
GO

-- Gera os comandos UPDATE com valores literais
-- Execute esta query e copie todos os resultados
SELECT 
    'UPDATE DadosCadastrais SET FormaPagamento = ''' + 
    REPLACE(Forma_pagamento, '''', '''''') + 
    ''' WHERE MatriculaAstel = ' + CAST(id_cliente AS VARCHAR) + ';' AS ScriptSQL
FROM formapgto
WHERE id_cliente IS NOT NULL
ORDER BY id_cliente;
GO

-- Para gerar o script completo automaticamente, execute o bloco abaixo:
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + 
    'UPDATE DadosCadastrais SET FormaPagamento = ''' + 
    REPLACE(Forma_pagamento, '''', '''''') + 
    ''' WHERE MatriculaAstel = ' + CAST(id_cliente AS VARCHAR) + ';' + CHAR(13) + CHAR(10)
FROM formapgto
WHERE id_cliente IS NOT NULL
ORDER BY id_cliente;

-- Exibe o script completo
PRINT 'USE ASTEL;';
PRINT 'GO';
PRINT '';
PRINT @sql;
PRINT '';
PRINT 'GO';

-- Também salva em uma variável para facilitar cópia
SELECT @sql AS ScriptCompleto;
GO

