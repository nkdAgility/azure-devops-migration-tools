using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;
using NCalc;

namespace MigrationTools.Tests.Tools.FieldMaps
{
    [TestClass]
    public class FieldCalculationMapTests
    {
        [TestMethod]
        public void FieldCalculationMapOptions_Constructor_ShouldInitializeParametersDictionary()
        {
            // Arrange & Act
            var options = new FieldCalculationMapOptions();

            // Assert
            Assert.IsNotNull(options.parameters);
            Assert.IsEmpty(options.parameters);
        }

        [TestMethod]
        public void FieldCalculationMapOptions_SetExampleConfigDefaults_ShouldSetCorrectValues()
        {
            // Arrange
            var options = new FieldCalculationMapOptions();

            // Act
            options.SetExampleConfigDefaults();

            // Assert
            Assert.AreEqual("[x]*2", options.expression);
            Assert.AreEqual("Custom.FieldC", options.targetField);
            Assert.HasCount(1, options.parameters);
            Assert.AreEqual("Custom.FieldB", options.parameters["x"]);
            CollectionAssert.Contains(options.ApplyTo, "SomeWorkItemType");
        }

        [TestMethod]
        public void NCalcExpression_SimpleCalculation_ShouldEvaluateCorrectly()
        {
            // Arrange
            var expression = new Expression("[x] * 2");
            expression.Parameters["x"] = 5;

            // Act
            var result = expression.Evaluate();

            // Assert
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void NCalcExpression_ComplexCalculation_ShouldEvaluateCorrectly()
        {
            // Arrange
            var expression = new Expression("([a] + [b]) * [c]");
            expression.Parameters["a"] = 10;
            expression.Parameters["b"] = 5;
            expression.Parameters["c"] = 2;

            // Act
            var result = expression.Evaluate();

            // Assert
            Assert.AreEqual(30, result);
        }

        [TestMethod]
        public void NCalcExpression_DivisionCalculation_ShouldEvaluateCorrectly()
        {
            // Arrange
            var expression = new Expression("[total] / [count]");
            expression.Parameters["total"] = 100;
            expression.Parameters["count"] = 4;

            // Act
            var result = expression.Evaluate();

            // Assert - NCalc returns double for division
            Assert.AreEqual(25.0, result);
        }

        [TestMethod]
        public void NCalcExpression_MathFunctions_ShouldEvaluateCorrectly()
        {
            // Arrange - Test Abs function alone to avoid type conflicts
            var expression = new Expression("Abs([value])");
            expression.Parameters["value"] = -25;

            // Act
            var result = expression.Evaluate();

            // Assert - NCalc returns decimal for Abs
            Assert.AreEqual(25m, result);
        }

        [TestMethod]
        public void NCalcExpression_SqrtFunction_ShouldEvaluateCorrectly()
        {
            // Arrange - Test Sqrt function separately
            var expression = new Expression("Sqrt([square])");
            expression.Parameters["square"] = 16.0;

            // Act
            var result = expression.Evaluate();

            // Assert
            Assert.AreEqual(4.0, result);
        }

        [TestMethod]
        public void NCalcExpression_InvalidExpression_ShouldThrowException()
        {
            // Arrange
            var expression = new Expression("[x] + [");  // Invalid syntax

            // Act & Assert - NCalc throws EvaluationException for invalid syntax
            Assert.Throws<NCalc.EvaluationException>(() => expression.Evaluate());
        }

        [TestMethod]
        public void NCalcExpression_UndefinedVariable_ShouldThrowException()
        {
            // Arrange
            var expression = new Expression("[undefined] * 2");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => expression.Evaluate());
        }
    }
}
