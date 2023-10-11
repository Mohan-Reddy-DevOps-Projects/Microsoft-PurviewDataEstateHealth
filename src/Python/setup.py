# coding: utf-8

"""
    Data Estate Health Service
"""

from setuptools import setup, find_packages

NAME = "DataEstateHealthLibrary"
VERSION = "0.0.3"
# To install the library, run the following
#
# python setup.py install
#
# prerequisite: setuptools
# http://pypi.python.org/pypi/setuptools

REQUIRES = ["urllib3 >= 1.15", "six >= 1.10", "certifi", "python-dateutil", "pyspark >= 3.1.2", "delta-spark == 2.4.0"]

setup(
    name=NAME,
    version=VERSION,
    description="Data Estate Health Python Library",
    author_email="",
    url="",
    keywords=["Swagger", "Purview Catalog Service REST API Document"],
    install_requires=REQUIRES,
    packages=find_packages(),
    include_package_data=True,
    classifiers=["Development - Alpha"],
    long_description="""\
    Data Estate Health Service # noqa: E501
    """
)
