#!/bin/bash
# FlightTracker3D Service Setup Script
# This script installs and configures the systemd services for Readsb and FlightTracker3D

set -e  # Exit on error

echo "=========================================="
echo "FlightTracker3D Service Setup"
echo "=========================================="

# Check if running as pi user
if [ "$USER" != "pi" ]; then
    echo "Warning: This script is designed to run as user 'pi'"
fi

# Add user to required groups for hardware access
echo "Adding user 'pi' to gpio, i2c, and spi groups..."
sudo usermod -a -G gpio,i2c,spi pi

# Install Readsb service first
echo "Installing Readsb systemd service file..."
sudo cp readsb.service /etc/systemd/system/
sudo chmod 644 /etc/systemd/system/readsb.service

# Install FlightTracker3D service
echo "Installing FlightTracker3D systemd service file..."
sudo cp flighttracker3d.service /etc/systemd/system/
sudo chmod 644 /etc/systemd/system/flighttracker3d.service

# Reload systemd daemon
echo "Reloading systemd daemon..."
sudo systemctl daemon-reload

# Enable services to start at boot
echo "Enabling Readsb service to start at boot..."
sudo systemctl enable readsb.service

echo "Enabling FlightTracker3D service to start at boot..."
sudo systemctl enable flighttracker3d.service

# Start Readsb service first
echo "Starting Readsb service..."
sudo systemctl start readsb.service

# Wait a moment for Readsb to initialize
echo "Waiting for Readsb to initialize..."
sleep 5

# Start FlightTracker3D service (will automatically wait 4 seconds due to ExecStartPre)
echo "Starting FlightTracker3D service..."
sudo systemctl start flighttracker3d.service

# Wait a moment for services to start
sleep 2

# Show service status
echo ""
echo "=========================================="
echo "Readsb Service Status:"
echo "=========================================="
sudo systemctl status readsb.service --no-pager

echo ""
echo "=========================================="
echo "FlightTracker3D Service Status:"
echo "=========================================="
sudo systemctl status flighttracker3d.service --no-pager

echo ""
echo "=========================================="
echo "Setup Complete!"
echo "=========================================="
echo ""
echo "Useful commands:"
echo ""
echo "Readsb:"
echo "  View logs:       sudo journalctl -u readsb.service -f"
echo "  Stop service:    sudo systemctl stop readsb.service"
echo "  Start service:   sudo systemctl start readsb.service"
echo "  Restart service: sudo systemctl restart readsb.service"
echo "  Service status:  sudo systemctl status readsb.service"
echo ""
echo "FlightTracker3D:"
echo "  View logs:       sudo journalctl -u flighttracker3d.service -f"
echo "  Stop service:    sudo systemctl stop flighttracker3d.service"
echo "  Start service:   sudo systemctl start flighttracker3d.service"
echo "  Restart service: sudo systemctl restart flighttracker3d.service"
echo "  Service status:  sudo systemctl status flighttracker3d.service"
echo ""
echo "Stop both services:"
echo "  sudo systemctl stop flighttracker3d.service readsb.service"
echo ""
echo "Start both services:"
echo "  sudo systemctl start readsb.service"
echo "  (FlightTracker3D will start automatically after 4 seconds)"
echo ""
echo "Disable auto-start:"
echo "  sudo systemctl disable flighttracker3d.service"
echo "  sudo systemctl disable readsb.service"
echo ""
echo "Note: You may need to log out and back in for group changes to take effect."
echo "      Or reboot the Raspberry Pi for services to start automatically."
echo ""

