# BotScraper V2
![](http://bytevaultstudio.se/ShareX/CB5qyH6HSc.gif)

This scraper project is the second evolution of BotScraper.
The first bot was simply a console window doing synchronously work on one single thread and had no command input or different colorations of the text.

This project however was created in order to make it a bit nicer with text colors, logging, threads, command inputs etc.

So, Why did you make this software?
Most of my recent projects has been involving twitch. Either I have worked with the Twitch API or the Twitch chat (TMI).
So I wanted to be able to distinguish between real chatters and bots. The API or chatters.json doesnt seperate the bots from the real chaters.
This is where I started searching the internet for a solution but I came across a website which had a list. This website used json.

Now, I could use this json data straight of the website BUT it had flaws in my opinion. 
The update frequency was fluxuating heavy and was unreliable in it's updates; Meaning that the data was only accurate for the coming 5 minutes after update.

This is why I made this software. I made this software to create a more accurate and reliable database of active twitch bots.
This software will scrape the data and add to my list of unique bots. 
If the bot doesnt exist it gets added.
If the bot does exist it gets updated.
if the bot has not been seen for 7 days it gets marked as inactive and removed from the database.

It's highly personalized for my needs but it should be pretty easy to modify for anyone interested in some of the base features or methods.
(This project was made because I was bored and needed to work on something else for a while.)
