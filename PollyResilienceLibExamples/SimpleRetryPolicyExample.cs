using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PollyResilienceLibExamples
{
    public class SimpleRetryPolicyExample
    {
        public void CreateRetryPolicyAndExecuteDelegate()
        {
            var retryPolicy = Policy
            //HandleResult method will create RetryPolly<T> and this handler will handle all httpResponseMessages whose status code is different than OK and will retry the execution call
            .HandleResult<HttpResponseMessage>((r) => r.StatusCode != System.Net.HttpStatusCode.OK)
            //here three sleep durations between trials are given and this also means the number of trials, 3.
            //by doing so the sleep durations between durations can be different as in this example.
            .WaitAndRetry(
                new TimeSpan[] { TimeSpan.FromSeconds(1),
                                 TimeSpan.FromSeconds(2),
                                 TimeSpan.FromSeconds(3) },
                onRetry: (ex, span, i, ctx) => Console.WriteLine("\nRetrying...  Waiting " + span.TotalMilliseconds + "ms first. Retry " + i + " next.")
             );

            //the number of attempts and the sleep duration can be given such as here, however all sleep durations between trials will be same.
            //.WaitAndRetry(
            //    retryCount: 3,
            //    sleepDurationProvider: i => TimeSpan.FromSeconds(1),
            // );

            //there is no need to use httpClient here since the result of http call is simulated and this httpresponse is badrequest
            //so the retryPolicy will attempt three trials and at the third attempt it returns the httpResponseMessage(badRequest) as it is, it will not throw any exception
            retryPolicy.Execute(() => new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest });
        }

        public void CreateRetryPolicyWithTimeoutPolicyAndExecuteDelegate()
        {
            var retryPolicy = Policy
            //HandleResult method will create RetryPolly<T> and this handler will handle all httpResponseMessages whose status code is different than OK and will retry the execution call
            .HandleResult<HttpResponseMessage>((r) => r.StatusCode != System.Net.HttpStatusCode.OK)
            //here three sleep durations between trials are given and this also means the number of trials, 3.
            //by doing so the sleep durations between durations can be different as in this example.
            .WaitAndRetry(
                new TimeSpan[] { TimeSpan.FromSeconds(1),
                                 TimeSpan.FromSeconds(2),
                                 TimeSpan.FromSeconds(3) },
                onRetry: (ex, span, i, ctx) => Console.WriteLine("\nRetrying...  Waiting " + span.TotalMilliseconds + "ms first. Retry " + i + " next.")
             );

            //if execution of trials are bigger than 3 seconds the Polly.TimeoutRejectionException is thrown for every trial but retry policy will not throw any exception
            //untill the end of last trial and then will throw also TimeoutRejectionException, so if you use timeout policy you can wrap your execute function by try-catch
            var wrapPolicy = retryPolicy.Wrap(Policy.Timeout(3, Polly.Timeout.TimeoutStrategy.Pessimistic));

            //there is no need to use httpClient here since the result of http call is simulated and this httpresponse is badrequest
            //so the retryPolicy will attempt three trials and at the third attempt it returns the httpResponseMessage(badRequest) as it is, it will not throw any exception
            wrapPolicy.Execute(() => new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest });
        }

        public void CreateRetryPolicyExecuteDelegateAndHandleExceptionOrExecution()
        {
            //here if no exception occurs but if httpresponse message status is different retry policy will make attempts to retry the execution
            //if any exception occures retry policy will make attempts to retry the execution
            var retryPolicy = Policy
            //HandleResult method will create RetryPolly<T> and this handler will handle all httpResponseMessages whose status code is different than OK and will retry the execution call
            .HandleResult<HttpResponseMessage>((r) => r.StatusCode != System.Net.HttpStatusCode.OK)
            .Or<Exception>()//every exception will be handled and retry policy will make attempts
            //here three sleep durations between trials are given and this also means the number of trials, 3.
            //by doing so the sleep durations between durations can be different as in this example.
            .WaitAndRetry(
                new TimeSpan[] { TimeSpan.FromSeconds(1),
                                 TimeSpan.FromSeconds(2),
                                 TimeSpan.FromSeconds(3) },
                onRetry: (ex, span, i, ctx) => Console.WriteLine("\nRetrying...  Waiting " + span.TotalMilliseconds + "ms first. Retry " + i + " next.")
             );

            //there is no need to use httpClient here since the result of http call is simulated and this httpresponse is badrequest
            //so the retryPolicy will attempt three trials and at the third attempt it returns the httpResponseMessage(badRequest) as it is, it will not throw any exception
            retryPolicy.Execute(() => new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest });
        }

        public void CreateRetryPolicyHandleOnlyExceptions()
        {
            //here retry policy only handle exceptions and this retry policy is nongeneric policy RetryPolicy, extended from Polly.
            var retryPolicy = Policy
            .Handle<Exception>()//every exception will be handled and retry policy will make attempts
            //here three sleep durations between trials are given and this also means the number of trials, 3.
            //by doing so the sleep durations between durations can be different as in this example.
            .WaitAndRetry(
                new TimeSpan[] { TimeSpan.FromSeconds(1),
                                 TimeSpan.FromSeconds(2),
                                 TimeSpan.FromSeconds(3) },
                onRetry: (ex, span, i, ctx) => Console.WriteLine("\nRetrying...  Waiting " + span.TotalMilliseconds + "ms first. Retry " + i + " next.")
             );

            //Even you can add HandleResult(actually OrResult for the example below commented out) after Handle to handle custom conditions
            //Handle method will return nongeneric PolicyBuilder. OrResult will return generic PolicyBuilder<T>. Then this generic PolicyBuilder calls WaitAndRetry
            //and this will create generic RetryPolicy.
            //The switch between nongeneric policy to generic policy (or vice versa) is done by PolicyBuilder objects.

            //var GenericPolicyBuilder = Policy
            //.Handle<Exception>()
            //.OrResult<HttpResponseMessage>((r) => r.StatusCode != System.Net.HttpStatusCode.OK);

            //var retryPolicy2 = GenericPolicyBuilder.WaitAndRetry(
            //    new TimeSpan[] { TimeSpan.FromSeconds(1),
            //                     TimeSpan.FromSeconds(2),
            //                     TimeSpan.FromSeconds(3) },
            //    onRetry: (ex, span, i, ctx) => Console.WriteLine("\nRetrying...  Waiting " + span.TotalMilliseconds + "ms first. Retry " + i + " next.")
            // );

            //there is no need to use httpClient here since the result of http call is simulated and this httpresponse is badrequest
            //so the retryPolicy will attempt three trials and at the third attempt it returns the httpResponseMessage(badRequest) as it is, it will not throw any exception
            retryPolicy.Execute<HttpResponseMessage>(() => {
                throw new Exception("exception occured");
            });
        }
    }
}
