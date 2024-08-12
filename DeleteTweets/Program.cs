using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;

var csrfToken = args[0];
var authToken = args[1];
var userId = args[2];

var client = new HttpClient(new HttpClientHandler { UseCookies = false });
client.DefaultRequestHeaders.Add("cookie", $@"lang=en; tweetdeck_version=legacy; dnt=1; ads_prefs=""HBESAAA=""; auth_token={authToken}; guest_id_ads=v1%3A{userId}; guest_id_marketing=v1%3A{userId}; guest_id=v1%3A{userId}; twid=u%3D{userId}; ct0={csrfToken};");
client.DefaultRequestHeaders.Add("x-csrf-token", csrfToken);
client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(@"Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");
client.BaseAddress = new Uri("https://x.com/i/api/graphql/");

List<string> tweetIds;

do
{
    var variables = HttpUtility.UrlEncode(
        $"{{\"count\":20,\"includePromotedContent\":false,\"withQuickPromoteEligibilityTweetFields\":false,\"withVoice\":true,\"withV2Timeline\":true,\"userId\":\"{userId}\"}}"
    );
    var features = HttpUtility.UrlEncode(
        "{\"rweb_lists_timeline_redesign_enabled\":false,\"responsive_web_graphql_exclude_directive_enabled\":true,\"verified_phone_label_enabled\":false,\"creator_subscriptions_tweet_preview_api_enabled\":true,\"responsive_web_graphql_timeline_navigation_enabled\":true,\"responsive_web_graphql_skip_user_profile_image_extensions_enabled\":false,\"tweetypie_unmention_optimization_enabled\":true,\"responsive_web_edit_tweet_api_enabled\":true,\"graphql_is_translatable_rweb_tweet_is_translatable_enabled\":true,\"view_counts_everywhere_api_enabled\":true,\"longform_notetweets_consumption_enabled\":true,\"responsive_web_twitter_article_tweet_consumption_enabled\":false,\"tweet_awards_web_tipping_enabled\":false,\"freedom_of_speech_not_reach_fetch_enabled\":true,\"standardized_nudges_misinfo\":true,\"tweet_with_visibility_results_prefer_gql_limited_actions_policy_enabled\":true,\"longform_notetweets_rich_text_read_enabled\":true,\"longform_notetweets_inline_media_enabled\":true,\"responsive_web_media_download_video_enabled\":false,\"responsive_web_enhance_cards_enabled\":false}"
    );
    try
    {
        var timeline = await client.GetFromJsonAsync<JsonObject>(
            $"wxoVeDnl0mP7VLhe6mTOdg/UserTweetsAndReplies?variables={variables}&features={features}"
        );
        var timelineEntries = timeline["data"]["user"]["result"]["timeline_v2"]["timeline"]["instructions"]
            .AsArray()
            .First(x => (string)x["type"] == "TimelineAddEntries")["entries"]
            .AsArray();

        tweetIds = timelineEntries
            .Select(x => Regex.Match((string)x["entryId"], "(?<=tweet-)\\d+").Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Concat(
                timelineEntries
                    .Where(x => Regex.IsMatch((string)x["entryId"], "(?<=profile-conversation-)\\d+"))
                    .SelectMany(x => x["content"]["items"].AsArray(), (_, x) => x["item"]["itemContent"])
                    .Where(x => (string)x["itemType"] == "TimelineTweet")
                    .Select(x => x["tweet_results"]["result"])
                    .Where(x => (string)x["core"]["user_results"]["result"]["rest_id"] == userId)
                    .Select(x => (string)x["rest_id"])
            )
            .ToList();

        foreach (var tweetId in tweetIds)
        {
            var deleteQueryId = "VaenaVgh5q5ih7kvyVjgtg";
            var response = await client.PostAsJsonAsync(
                $"{deleteQueryId}/DeleteTweet",
                new { variables = new { tweet_id = tweetId, dark_request = false }, queryId = deleteQueryId }
            );
            try
            {
                response.EnsureSuccessStatusCode();
                Console.WriteLine($"Deleted post {tweetId}");
            }
            catch
            {
                Console.WriteLine($"Error: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        return;
    }
    
} while (tweetIds.Any());
