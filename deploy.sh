#!/usr/bin/env bash

source ../CONFIG.inc

VERSIONFILE=ImpossibleInnovations.version

scp -i $SSH_ID ./GameData/net.lisias.ksp/$VERSIONFILE $SITE:/$TARGETPATH