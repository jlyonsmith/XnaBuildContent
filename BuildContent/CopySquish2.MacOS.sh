CONFIG=$1
if [ -z $CONFIG ]; then CONFIG=Debug; fi
cp ../Squish2/Squish2/bin/$CONFIG/libSquish2.so bin/$CONFIG/
