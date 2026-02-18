/*
This example shows how to use the Option monad in a stream scenario.
By building a pipeline with a set of conditions and then evaluating the results with Match.
*/

using Svan.Monads;

StreamOfIntegers(randomInteger =>
{
    var result = randomInteger.ToOption()
        .Filter(i => i > 50)
        .Map(i => i * 99)
        .Bind(i => i % 2 == 0 ? Option<int>.Some(i) : Option<int>.None())
        .Map(some => $"{randomInteger} is above 50 and becomes the even number {some} when multiplied by 99")
        .DefaultWith(() => "did not pass checks");

    Console.WriteLine(result);
});

// Produces random integers between 1 and 100
static void StreamOfIntegers(Action<int> callback)
{
    const int SleepMsBetweenCallbacks = 500;
    var rng = new Random(Seed: 1);
    while (true)
    {
        callback(rng.Next(100) + 1);
        Thread.Sleep(SleepMsBetweenCallbacks);
    }
}
