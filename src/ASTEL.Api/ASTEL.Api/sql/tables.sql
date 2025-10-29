-- Cria o banco de dados ASTEL se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ASTEL')
BEGIN
    CREATE DATABASE ASTEL;
END
GO

USE ASTEL;
GO

-- Tabela DadosCadastrais (sem alterações)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DadosCadastrais')
BEGIN
    CREATE TABLE DadosCadastrais (
        MatriculaSistel BIGINT PRIMARY KEY,
        MatriculaAstel BIGINT NOT NULL UNIQUE,
        Nome VARCHAR(120) NOT NULL,
        Endereco VARCHAR(255),
        Situacao INT,
        ValorBeneficio FLOAT,
        EstadoCivil VARCHAR(50),
        Telefone VARCHAR(20),
        NomeEsposa VARCHAR(120),
        CPF VARCHAR(14),
        RG VARCHAR(20),
        Ativo BIT,
        DescontoFolha BIT
    );
END
GO

-- 🔹 Tabela DadosFinanceiros (com novo campo ValorPago)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DadosFinanceiros')
BEGIN
    CREATE TABLE DadosFinanceiros (
        MatriculaSistel BIGINT,
        MatriculaAstel BIGINT,
        Ano INT,
        Mes FLOAT,
        ValorPago FLOAT NULL, -- 🔸 Novo campo
        FOREIGN KEY (MatriculaSistel) REFERENCES DadosCadastrais(MatriculaSistel),
        PRIMARY KEY (MatriculaSistel, MatriculaAstel, Mes, Ano)
    );
END
GO

-- Caso a tabela já exista, adiciona a coluna se faltar
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE Name = N'ValorPago' AND Object_ID = Object_ID(N'DadosFinanceiros')
)
BEGIN
    ALTER TABLE DadosFinanceiros ADD ValorPago FLOAT NULL;
END
GO
