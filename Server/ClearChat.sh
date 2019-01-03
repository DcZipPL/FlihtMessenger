#/bin/bash

rm /var/www/html/Server/Messages.dat
touch /var/www/html/Server/Messages.dat
chmod o+rw /var/www/html/Server/Messages.dat
echo "Cleared Chat!"
