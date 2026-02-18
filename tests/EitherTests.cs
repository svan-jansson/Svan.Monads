using Xunit;

namespace Svan.Monads.UnitTests
{
    public class EitherTests
    {
        [Fact]
        public void FromLeft_creates_a_left_either()
        {
            var either = Either<string, int>.FromLeft("error");
            Assert.True(either.IsLeft);
            Assert.False(either.IsRight);
            Assert.Equal("error", either.LeftValue());
        }

        [Fact]
        public void FromRight_creates_a_right_either()
        {
            var either = Either<string, int>.FromRight(42);
            Assert.True(either.IsRight);
            Assert.False(either.IsLeft);
            Assert.Equal(42, either.RightValue());
        }

        [Fact]
        public void Map_transforms_right_value()
        {
            var either = Either<string, int>.FromRight(5);
            var result = either.Map(x => x * 2);
            Assert.True(result.IsRight);
            Assert.Equal(10, result.RightValue());
        }

        [Fact]
        public void Map_short_circuits_on_left()
        {
            var either = Either<string, int>.FromLeft("error");
            var result = either.Map(x => x * 2);
            Assert.True(result.IsLeft);
            Assert.Equal("error", result.LeftValue());
        }

        [Fact]
        public void MapLeft_transforms_left_value()
        {
            var either = Either<string, int>.FromLeft("error");
            var result = either.MapLeft(s => s.Length);
            Assert.True(result.IsLeft);
            Assert.Equal(5, result.LeftValue());
        }

        [Fact]
        public void BindLeft_chains_left_values()
        {
            var either = Either<string, int>.FromLeft("error");
            var result = either.BindLeft(l => Either<int, int>.FromLeft(l.Length));
            Assert.True(result.IsLeft);
            Assert.Equal(5, result.LeftValue());
        }

        [Fact]
        public void BindLeft_short_circuits_on_right()
        {
            var either = Either<string, int>.FromRight(42);
            var wasCalled = false;
            var result = either.BindLeft(l => { wasCalled = true; return Either<int, int>.FromLeft(l.Length); });
            Assert.False(wasCalled);
            Assert.True(result.IsRight);
            Assert.Equal(42, result.RightValue());
        }

        [Fact]
        public void MapLeft_short_circuits_on_right()
        {
            var either = Either<string, int>.FromRight(42);
            var result = either.MapLeft(s => s.Length);
            Assert.True(result.IsRight);
            Assert.Equal(42, result.RightValue());
        }

        [Fact]
        public void Bind_chains_right_values()
        {
            var either = Either<string, int>.FromRight(5);
            var result = either.Bind(x => Either<string, int>.FromRight(x + 3));
            Assert.True(result.IsRight);
            Assert.Equal(8, result.RightValue());
        }

        [Fact]
        public void Bind_short_circuits_on_left()
        {
            var either = Either<string, int>.FromLeft("error");
            var wasCalled = false;
            var result = either.Bind(x => { wasCalled = true; return Either<string, int>.FromRight(x + 3); });
            Assert.False(wasCalled);
            Assert.True(result.IsLeft);
            Assert.Equal("error", result.LeftValue());
        }

        [Fact]
        public void Do_fires_only_on_right()
        {
            var called = false;
            Either<string, int>.FromRight(5)
                .Do(x => { called = true; Assert.Equal(5, x); });
            Assert.True(called);

            Either<string, int>.FromLeft("error")
                .Do(_ => Assert.Fail("Should not be called on left"));
        }

        [Fact]
        public void DoIfLeft_fires_only_on_left()
        {
            var called = false;
            Either<string, int>.FromLeft("error")
                .DoIfLeft(x => { called = true; Assert.Equal("error", x); });
            Assert.True(called);

            Either<string, int>.FromRight(5)
                .DoIfLeft(_ => Assert.Fail("Should not be called on right"));
        }

        [Fact]
        public void Fold_calls_correct_branch()
        {
            var rightResult = Either<string, int>.FromRight(5)
                .Fold(l => -1, r => r * 2);
            Assert.Equal(10, rightResult);

            var leftResult = Either<string, int>.FromLeft("error")
                .Fold(l => l.Length, r => r * 2);
            Assert.Equal(5, leftResult);
        }

        [Fact]
        public void DefaultWith_func_returns_right_value_or_fallback_from_left()
        {
            Assert.Equal(42, Either<string, int>.FromRight(42).DefaultWith(l => l.Length));
            Assert.Equal(5, Either<string, int>.FromLeft("error").DefaultWith(l => l.Length));
        }

        [Fact]
        public void DefaultWith_value_returns_right_value_or_fallback()
        {
            Assert.Equal(42, Either<string, int>.FromRight(42).DefaultWith(0));
            Assert.Equal(0, Either<string, int>.FromLeft("error").DefaultWith(0));
        }

        [Fact]
        public void OrThrow_on_left_throws_invalid_operation_exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Either<string, int>.FromLeft("error").OrThrow());
        }

        [Fact]
        public void OrThrow_on_right_returns_value()
        {
            Assert.Equal(42, Either<string, int>.FromRight(42).OrThrow());
        }

        [Fact]
        public void Swap_flips_sides()
        {
            var swappedRight = Either<string, int>.FromRight(42).Swap();
            Assert.True(swappedRight.IsLeft);
            Assert.Equal(42, swappedRight.LeftValue());

            var swappedLeft = Either<string, int>.FromLeft("error").Swap();
            Assert.True(swappedLeft.IsRight);
            Assert.Equal("error", swappedLeft.RightValue());
        }

        [Fact]
        public void Zip_combines_two_rights()
        {
            var result = Either<string, int>.FromRight(5)
                .Zip(Either<string, int>.FromRight(3), (a, b) => a + b);
            Assert.True(result.IsRight);
            Assert.Equal(8, result.RightValue());
        }

        [Fact]
        public void Zip_returns_first_left_encountered()
        {
            var result = Either<string, int>.FromRight(5)
                .Zip(Either<string, int>.FromLeft("first error"), (a, b) => a + b);
            Assert.True(result.IsLeft);
            Assert.Equal("first error", result.LeftValue());

            var result2 = Either<string, int>.FromLeft("first error")
                .Zip(Either<string, int>.FromLeft("second error"), (a, b) => a + b);
            Assert.True(result2.IsLeft);
            Assert.Equal("first error", result2.LeftValue());
        }

        [Fact]
        public void Zip_five_combines_all_rights()
        {
            var result = Either<string, int>.FromRight(1)
                .Zip(
                    Either<string, int>.FromRight(2),
                    Either<string, int>.FromRight(3),
                    Either<string, int>.FromRight(4),
                    Either<string, int>.FromRight(5),
                    (a, b, c, d, e) => a + b + c + d + e);
            Assert.True(result.IsRight);
            Assert.Equal(15, result.RightValue());
        }

        [Fact]
        public void Sequence_returns_right_with_all_values_when_all_right()
        {
            var eithers = new[]
            {
                Either<string, int>.FromRight(1),
                Either<string, int>.FromRight(2),
                Either<string, int>.FromRight(3),
            };

            var result = eithers.Sequence();
            Assert.True(result.IsRight);
            Assert.Equal(new[] { 1, 2, 3 }, result.RightValue());
        }

        [Fact]
        public void Sequence_returns_first_left_when_any_is_left()
        {
            var eithers = new[]
            {
                Either<string, int>.FromRight(1),
                Either<string, int>.FromLeft("first error"),
                Either<string, int>.FromLeft("second error"),
            };

            var result = eithers.Sequence();
            Assert.True(result.IsLeft);
            Assert.Equal("first error", result.LeftValue());
        }

        [Fact]
        public void Flatten_returns_inner_right_when_outer_is_right()
        {
            var nested = Either<string, Either<string, int>>.FromRight(Either<string, int>.FromRight(42));
            var flat = nested.Flatten();
            Assert.True(flat.IsRight);
            Assert.Equal(42, flat.RightValue());
        }

        [Fact]
        public void Flatten_propagates_outer_left()
        {
            var nested = Either<string, Either<string, int>>.FromLeft("outer error");
            var flat = nested.Flatten();
            Assert.True(flat.IsLeft);
            Assert.Equal("outer error", flat.LeftValue());
        }

        [Fact]
        public void Flatten_propagates_inner_left_when_outer_is_right()
        {
            var nested = Either<string, Either<string, int>>.FromRight(Either<string, int>.FromLeft("inner error"));
            var flat = nested.Flatten();
            Assert.True(flat.IsLeft);
            Assert.Equal("inner error", flat.LeftValue());
        }

        [Fact]
        public void ToOption_converts_right_to_some()
        {
            Either<string, int>.FromRight(42).ToOption().AssertSome(42);
        }

        [Fact]
        public void ToOption_converts_left_to_none()
        {
            Either<string, int>.FromLeft("error").ToOption().AssertNone();
        }

        [Fact]
        public void ToResult_converts_right_to_success()
        {
            Either<string, int>.FromRight(42).ToResult().AssertSuccess(42);
        }

        [Fact]
        public void ToResult_converts_left_to_error()
        {
            Either<string, int>.FromLeft("error").ToResult().AssertError("error");
        }

        [Fact]
        public void Merge_can_combine_eithers()
        {
            var result = Either<string, int>.FromRight(10)
                .Merge(Either<string, int>.FromRight(20))
                .Merge(Either<string, int>.FromRight(30))
                .Merge(Either<string, int>.FromRight(40))
                .Merge(Either<string, int>.FromRight(50))
                .Fold(
                    _ => 0,
                    group => group.Item1 + group.Item2 + group.Item3 + group.Item4 + group.Item5);

            Assert.Equal(150, result);
        }
    }
}
