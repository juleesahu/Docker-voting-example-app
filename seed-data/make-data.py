import urllib.parse

# Function to create a URL-encoded POST data file
def create_post_file(filename, vote_option):
    params = {'vote': vote_option}
    encoded = urllib.parse.urlencode(params)
    
    # Use 'with' to ensure file is properly closed
    with open(filename, 'w') as outfile:
        outfile.write(encoded)

# Create postb file with vote option 'b'
create_post_file('postb', 'b')

# Create posta file with vote option 'a'
create_post_file('posta', 'a')

print("POST data files created successfully.")
