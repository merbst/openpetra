-- passwords are inserted by OpenPetra.build
INSERT INTO s_user(s_user_id_c, s_password_hash_c, s_password_salt_c, s_password_needs_change_l) VALUES('SYSADMIN', '{#PASSWORDHASHSYSADMIN}', '{#PASSWORDSALTSYSADMIN}', true);

-- load base data
COPY s_logon_message FROM '{#ABSOLUTEBASEDATAPATH}/s_logon_message.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_location_type FROM '{#ABSOLUTEBASEDATAPATH}/p_location_type.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_international_postal_type FROM '{#ABSOLUTEBASEDATAPATH}/p_international_postal_type.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_country FROM '{#ABSOLUTEBASEDATAPATH}/p_country.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_language FROM '{#ABSOLUTEBASEDATAPATH}/p_language.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_addressee_type FROM '{#ABSOLUTEBASEDATAPATH}/p_addressee_type.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY a_currency FROM '{#ABSOLUTEBASEDATAPATH}/a_currency.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_acquisition FROM '{#ABSOLUTEBASEDATAPATH}/p_acquisition.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY s_module FROM '{#ABSOLUTEBASEDATAPATH}/s_module.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_type FROM '{#ABSOLUTEBASEDATAPATH}/p_type.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_partner_status FROM '{#ABSOLUTEBASEDATAPATH}/p_partner_status.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_partner_classes FROM '{#ABSOLUTEBASEDATAPATH}/p_partner_classes.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY pt_marital_status FROM '{#ABSOLUTEBASEDATAPATH}/pt_marital_status.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY p_location FROM '{#ABSOLUTEBASEDATAPATH}/p_location.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY a_sub_system FROM '{#ABSOLUTEBASEDATAPATH}/a_sub_system.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY a_budget_type FROM '{#ABSOLUTEBASEDATAPATH}/a_budget_type.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';
COPY a_frequency FROM '{#ABSOLUTEBASEDATAPATH}/a_frequency.csv' WITH DELIMITER AS ',' NULL AS '?' CSV QUOTE AS '"' ESCAPE AS '"';

INSERT INTO s_system_status(s_user_id_c,s_system_login_status_l) VALUES('SYSADMIN', true);
INSERT INTO p_partner(p_partner_key_n, p_partner_short_name_c) VALUES(0, 'INVALID PARTNER');
INSERT INTO s_system_defaults(s_default_code_c, s_default_description_c, s_default_value_c) VALUES ('LocalisedCountyLabel', 'LocalisedCountyLabel', 'County/St&ate');

-- setup initial user permissions
INSERT INTO s_user_module_access_permission(s_user_id_c,s_module_id_c,s_can_access_l) VALUES('SYSADMIN', 'SYSMAN', true);

INSERT INTO s_system_status(s_user_id_c,s_system_login_status_l) VALUES('SYSADMIN', true);
INSERT INTO p_partner(p_partner_key_n, p_partner_short_name_c) VALUES(0, 'INVALID PARTNER');
INSERT INTO s_system_defaults(s_default_code_c, s_default_description_c, s_default_value_c) VALUES ('LocalisedCountyLabel', 'LocalisedCountyLabel', 'County/St&ate');
