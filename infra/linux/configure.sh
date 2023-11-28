# Give app execution permission.
chmod +x /var/www/EcoHive/MrCapitalQ.EcoHive.Api

# Execute efbundle to create and update database.
chmod +x /var/www/EcoHive/efbundle
/var/www/EcoHive/efbundle --connection "Data Source=/var/www/EcoHive/ecobee.db"
chmod -R a+rw /var/www/EcoHive

# Optionally, stop and disable existing service.
sudo systemctl stop kestrel-ecohive.service
sudo systemctl disable kestrel-ecohive.service

# Copy latest service definition file to systemd.
sudo cp /var/www/EcoHive/kestrel-ecohive.service /etc/systemd/system
sudo systemctl daemon-reload

# Enable auto startup and start the service.
sudo systemctl enable kestrel-ecohive.service
sudo systemctl start kestrel-ecohive.service

# Optionally, check the status and logs of the service.
sudo systemctl status kestrel-ecohive.service
# journalctl -fu kestrel-ecohive.service --since today
