CONDA_PATH=~/miniconda3/bin/activate
CONDA_ENV='mlagents'
DEFAULT_CONFIG=./drone_agents_config.yml

WIDTH=640
HEIGHT=480
TIME_SCALE=1
RUN_ID=""

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

if [ -z "$RUN_ID" ]; then
  echo "Forcing new run ..." 
  mlagents-learn "$agent_config" --force --time-scale "$TIME_SCALE" --width "$WIDTH" --height "$HEIGHT"
else 
  echo "Trying to resume run '$RUN_ID' ..." 
  mlagents-learn "$agent_config" --resume --run-id "$RUN_ID" --time-scale "$TIME_SCALE" --width "$WIDTH" --height "$HEIGHT"
fi