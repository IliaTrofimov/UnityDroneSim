CONDA_PATH=~/miniconda3/bin/activate
CONDA_ENV='mlagents'
DEFAULT_CONFIG=./drone_agents_config.yml
agent_config=$1

if [ ! "$agent_config" ]; then
    echo "Empty ML agent configuration file! Using default config from '$DEFAULT_CONFIG'"
    agent_config=$DEFAULT_CONFIG
fi

echo "ML agent configuration file: '$agent_config'"

if [ ! -f "$agent_config" ]; then
    echo "ML agent configuration file is not found!"
    exit 1
fi

source $CONDA_PATH $CONDA_ENV
mlagents-learn "$agent_config" --resume

