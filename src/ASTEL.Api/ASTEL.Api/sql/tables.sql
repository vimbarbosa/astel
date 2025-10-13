-- Cria o banco de dados ASTEL se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ASTEL')
BEGIN
    CREATE DATABASE ASTEL;
END
GO

-- Usa o banco de dados ASTEL
USE ASTEL;
GO

-- Cria a tabela DadosCadastrais
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DadosCadastrais')
BEGIN
    CREATE TABLE DadosCadastrais (
        MatriculaSistel INT PRIMARY KEY,
        MatriculaAstel INT NOT NULL UNIQUE,
        Nome VARCHAR(120) NOT NULL,
        Endereco VARCHAR(255), -- Ajuste o tamanho conforme necessário
        Situacao INT, -- 1-TITULAR, 2-DEPENDENTE
        ValorBeneficio FLOAT,
        EstadoCivil VARCHAR(50), -- Ajuste o tamanho conforme necessário
        Telefone VARCHAR(20), -- Ajuste o tamanho conforme necessário
        NomeEsposa VARCHAR(120),
        CPF VARCHAR(14), -- Ajuste o tamanho conforme necessário
        RG VARCHAR(20), -- Ajuste o tamanho conforme necessário
        Ativo BIT, -- 1-true, 0-false
        DescontoFolha BIT -- TRUE OR FALSE
    );
END
GO

-- Cria a tabela DadosFinanceiros
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DadosFinanceiros')
BEGIN
    CREATE TABLE DadosFinanceiros (
        MatriculaSistel INT,
        MatriculaAstel INT,
        Ano INT,
        Mes FLOAT,
        FOREIGN KEY (MatriculaSistel) REFERENCES DadosCadastrais(MatriculaSistel),
		PRIMARY KEY (MatriculaSistel, MatriculaAstel, Ano)
    );
END
GO
