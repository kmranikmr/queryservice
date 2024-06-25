{
    "DBInstance": {
        "DBInstanceIdentifier": "digdagdb",
        "DBInstanceClass": "db.t3.micro",
        "Engine": "postgres",
        "DBInstanceStatus": "stopping",
        "MasterUsername": "ubuntu",
        "Endpoint": {
            "Address": "digdagdb.cv1llisahmax.us-east-1.rds.amazonaws.com",
            "Port": 5432,
            "HostedZoneId": "Z2R2ITUGPM61AM"
        },
        "AllocatedStorage": 20,
        "InstanceCreateTime": "2022-08-15T20:37:19.100000+00:00",
        "PreferredBackupWindow": "04:36-05:06",
        "BackupRetentionPeriod": 7,
        "DBSecurityGroups": [],
        "VpcSecurityGroups": [
            {
                "VpcSecurityGroupId": "sg-0e1d8c94fb1850629",
                "Status": "active"
            }
        ],
        "DBParameterGroups": [
            {
                "DBParameterGroupName": "dbcustom",
                "ParameterApplyStatus": "in-sync"
            }
        ],
        "AvailabilityZone": "us-east-1a",
        "DBSubnetGroup": {
            "DBSubnetGroupName": "default-vpc-04ef10156e1c4bdb3",
            "DBSubnetGroupDescription": "Created from the RDS Management Console",
            "VpcId": "vpc-04ef10156e1c4bdb3",
            "SubnetGroupStatus": "Complete",
            "Subnets": [
                {
                    "SubnetIdentifier": "subnet-0fd098985387507ca",
                    "SubnetAvailabilityZone": {
                        "Name": "us-east-1b"
                    },
                    "SubnetOutpost": {},
                    "SubnetStatus": "Active"
                },
                {
                    "SubnetIdentifier": "subnet-0c05c3621538d7a12",
                    "SubnetAvailabilityZone": {
                        "Name": "us-east-1a"
                    },
                    "SubnetOutpost": {},
                    "SubnetStatus": "Active"
                }
            ]
        },
        "PreferredMaintenanceWindow": "tue:03:10-tue:03:40",
        "PendingModifiedValues": {},
        "LatestRestorableTime": "2022-08-26T04:04:33+00:00",
        "MultiAZ": false,
        "EngineVersion": "14.2",
        "AutoMinorVersionUpgrade": true,
        "ReadReplicaDBInstanceIdentifiers": [],
        "LicenseModel": "postgresql-license",
        "OptionGroupMemberships": [
            {
                "OptionGroupName": "default:postgres-14",
                "Status": "in-sync"
            }
        ],
        "PubliclyAccessible": true,
        "StorageType": "gp2",
        "DbInstancePort": 0,
        "StorageEncrypted": true,
        "KmsKeyId": "arn:aws:kms:us-east-1:555972294483:key/3a24b917-332f-4465-ad63-d003e30db915",
        "DbiResourceId": "db-VWIEAIY6UZNKNAIBHQADA3SHO4",
        "CACertificateIdentifier": "rds-ca-2019",
        "DomainMemberships": [],
        "CopyTagsToSnapshot": true,
        "MonitoringInterval": 0,
        "DBInstanceArn": "arn:aws:rds:us-east-1:555972294483:db:digdagdb",
        "IAMDatabaseAuthenticationEnabled": false,
        "PerformanceInsightsEnabled": true,
        "PerformanceInsightsKMSKeyId": "arn:aws:kms:us-east-1:555972294483:key/3a24b917-332f-4465-ad63-d003e30db915",
        "PerformanceInsightsRetentionPeriod": 7,
        "DeletionProtection": false,
        "AssociatedRoles": [],
        "MaxAllocatedStorage": 50,
        "TagList": [],
        "CustomerOwnedIpEnabled": false
    }
}
