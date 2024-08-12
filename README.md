Usage: install .NET Core 8 and then run `DeleteTweets.exe csrfToken authToken userId`

You can get all three values by looking at your cookies (the former has key `ct0`) or your request headers. For the user ID, look at `twid` in your cookies, and omit `u%3D` at the beginning. I.e. `twid=u%3D1378720683807629316` means that my user ID is `1378720683807629316`

Expect to get an error 429 (Too Many Requests) after a while because the API is limited. Wait a little while, and run the program again

Known issues: doesn't delete the pinned tweet, you can do it yourself
