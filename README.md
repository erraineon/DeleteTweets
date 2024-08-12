Usage: `DeleteTweets.exe csrfToken authToken userId`

You can get all three values by looking at your cookies (the former has key `ct0`) or your request headers

Expect to get an error 429 (Too Many Requests) after a while because the API is limited. Wait a little while, and run the program again
