CONDA_PATH=~/miniconda3/bin/activate
CONDA_ENV='mlagents'
agent_config=$1

if [ ! "$agent_config" ]; then
    echo "Empty ML agent configuration file!"
    exit 1
fi

echo "ML agent configuration file: '$agent_config'"

if [ ! -f "$agent_config" ]; then
    echo "ML agent configuration file is not found!"
    exit 1
fi

source $CONDA_PATH $CONDA_ENV
mlagents-learn "$agent_config"

