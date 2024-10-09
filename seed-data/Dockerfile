FROM python:3.9-slim

# Install Apache Bench (ab) tool and clean up after the installation
RUN apt-get update && \
    apt-get install -y --no-install-recommends apache2-utils && \
    rm -rf /var/lib/apt/lists/*

# Set working directory inside the container
WORKDIR /seed

# Copy all files from the current directory on the host to the working directory in the container
COPY . .

# Run Python script to create POST data files in a format suitable for Apache Bench
RUN python3 make-data.py

# Define the default command to run the shell script for generating votes
CMD ["sh", "/seed/generate-votes.sh"]
