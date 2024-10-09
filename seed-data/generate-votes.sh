#!/bin/sh

# URL of the voting service
VOTE_URL="http://vote/"

# Define the number of requests and concurrency
NUM_REQUESTS=1000
CONCURRENCY=50

# File names for POST data (option 'a' and option 'b')
POST_A="posta"
POST_B="postb"

# Add votes for option 'a' (2000 votes total)
echo "Submitting 2000 votes for option 'a'..."
ab -n $NUM_REQUESTS -c $CONCURRENCY -p $POST_A -T "application/x-www-form-urlencoded" $VOTE_URL
ab -n $NUM_REQUESTS -c $CONCURRENCY -p $POST_A -T "application/x-www-form-urlencoded" $VOTE_URL

# Add votes for option 'b' (1000 votes total)
echo "Submitting 1000 votes for option 'b'..."
ab -n $NUM_REQUESTS -c $CONCURRENCY -p $POST_B -T "application/x-www-form-urlencoded" $VOTE_URL

echo "Vote submission completed."
