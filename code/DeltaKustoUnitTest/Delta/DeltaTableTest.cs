﻿using DeltaKustoLib;
using DeltaKustoLib.CommandModel;
using DeltaKustoLib.KustoModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Xunit;

namespace DeltaKustoUnitTest.Delta
{
    public class DeltaTableTest : ParsingTestBase
    {
        [Fact]
        public void FromEmptyToSomething()
        {
            var currentCommands = new CommandBase[0];
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = Parse(".create table t1(a: string, b: int)");
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Single(delta);
            Assert.IsType<CreateTableCommand>(delta[0]);
        }

        [Fact]
        public void FromSomethingToEmpty()
        {
            var currentCommands = Parse(".create table t1(a: string, b: int)");
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = new CommandBase[0];
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Single(delta);
            Assert.IsType<DropTableCommand>(delta[0]);
        }

        [Fact]
        public void NoChanges()
        {
            var currentCommands = Parse(".create table t1(a: string, b: int)");
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = Parse(".create table t1(a: string, b: int)");
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Empty(delta);
        }

        [Fact]
        public void AddingFolder()
        {
            var currentCommands = Parse(".create table t1(a: string, b: int) with (docstring='bla')");
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = Parse(".create table t1(a: string, b: int) with(docstring='bla', folder='abc')");
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Single(delta);
            Assert.IsType<CreateTableCommand>(delta[0]);

            var createTableCommand = (CreateTableCommand)delta[0];

            Assert.Equal(new QuotedText("abc"), createTableCommand.Folder);
            //  This hasn't changed so it shouldn't be part of the command
            Assert.Null(createTableCommand.DocString);
        }

        [Fact]
        public void RemovingDocString()
        {
            var currentCommands = Parse(".create table t1(a: string, b: int) with (docstring='bla')");
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = Parse(".create table t1(a: string, b: int)");
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Single(delta);
            Assert.IsType<CreateTableCommand>(delta[0]);

            var createTableCommand = (CreateTableCommand)delta[0];

            Assert.Null(createTableCommand.Folder);
            Assert.Equal(createTableCommand.DocString, new QuotedText(string.Empty));
        }

        [Fact]
        public void DropColumns()
        {
            var currentCommands = Parse(".create table t1(a: string, b: int, c:dynamic)");
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = Parse(".create table t1(b: int)");
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Single(delta);
            Assert.IsType<DropTableColumnsCommand>(delta[0]);

            var dropTableColumnsCommand = (DropTableColumnsCommand)delta[0];

            Assert.Equal(new EntityName("t1"), dropTableColumnsCommand.TableName);
            Assert.Contains(new EntityName("a"), dropTableColumnsCommand.ColumnNames);
            Assert.Contains(new EntityName("c"), dropTableColumnsCommand.ColumnNames);
        }

        [Fact]
        public void ChangeColumnType()
        {
            var currentCommands = Parse(".create table t1(a: string, b: int, c:dynamic)");
            var currentDatabase = DatabaseModel.FromCommands(currentCommands);
            var targetCommands = Parse(".create table t1(a: string, b:real, c:dynamic)");
            var targetDatabase = DatabaseModel.FromCommands(targetCommands);
            var delta = currentDatabase.ComputeDelta(targetDatabase);

            Assert.Single(delta);
            Assert.IsType<AlterColumnTypeCommand>(delta[0]);

            var alterColumnTypeCommand = (AlterColumnTypeCommand)delta[0];

            Assert.Equal(new EntityName("t1"), alterColumnTypeCommand.TableName);
            Assert.Equal(new EntityName("b"), alterColumnTypeCommand.ColumnName);
            Assert.Equal("real", alterColumnTypeCommand.Type);
        }
    }
}